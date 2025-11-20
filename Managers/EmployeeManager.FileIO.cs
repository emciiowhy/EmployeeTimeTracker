using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmployeeTimeTracker.Models;

namespace EmployeeTimeTracker.Managers
{
    public partial class EmployeeManager
    {
        private const string FILE_PATH = "employees.json";
        private const string TEMP_PATH = "employees.json.tmp";
        private const string BACKUP_PATH = "employees.json.bak";

        // Reused JSON settings (FAST + SAFE)
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            Converters = { new PolymorphicEmployeeConverter() }
        };

        // ---------------------------
        //        SAVE
        // ---------------------------
        public void SaveToFile()
        {
            try
            {
                // Serialize once with cached options
                string json = JsonSerializer.Serialize(employees, JsonOptions);

                // Write to temp file first → atomic write
                File.WriteAllText(TEMP_PATH, json);

                // Move with backup protection
                if (File.Exists(FILE_PATH))
                {
                    File.Replace(TEMP_PATH, FILE_PATH, BACKUP_PATH, ignoreMetadataErrors: true);
                }
                else
                {
                    File.Move(TEMP_PATH, FILE_PATH);
                }

                Console.WriteLine("Employees saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to save employees: {ex.Message}");
            }
        }

        // ---------------------------
        //        LOAD
        // ---------------------------
        public void LoadFromFile()
        {
            try
            {
                if (!File.Exists(FILE_PATH))
                {
                    Console.WriteLine("No saved employee data found.");
                    return;
                }

                string json = File.ReadAllText(FILE_PATH);

                var list = JsonSerializer.Deserialize<List<Employee>>(json, JsonOptions);

                if (list == null)
                {
                    Console.WriteLine("[WARNING] Employee file was empty or invalid — restoring backup.");
                    RestoreBackup();
                    return;
                }

                // Clear and reload in-memory list
                employees.Clear();
                employees.AddRange(list);

                Console.WriteLine($"Loaded {employees.Count} employees from file.");
            }
            catch (JsonException)
            {
                Console.WriteLine("[ERROR] Employee JSON corrupted — attempting backup restore.");
                RestoreBackup();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load employees: {ex.Message}");
            }
        }

        // ---------------------------
        //     BACKUP RESTORE
        // ---------------------------
        private void RestoreBackup()
        {
            try
            {
                if (!File.Exists(BACKUP_PATH))
                {
                    Console.WriteLine("[FATAL] No backup available. All employee data lost.");
                    return;
                }

                File.Copy(BACKUP_PATH, FILE_PATH, overwrite: true);

                Console.WriteLine("Backup restored. Re-loading employees...");

                LoadFromFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL] Backup restoration failed: {ex.Message}");
            }
        }
    }

    // ------------------------------------------
    //     Polymorphic Converter (unchanged)
    // ------------------------------------------
    public class PolymorphicEmployeeConverter : JsonConverter<Employee>
    {
        public override Employee? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            if (root.TryGetProperty("Type", out var tProp))
            {
                string? type = tProp.GetString();
                return type switch
                {
                    "FullTime" => JsonSerializer.Deserialize<FullTimeEmployee>(root.GetRawText(), options),
                    "PartTime" => JsonSerializer.Deserialize<PartTimeEmployee>(root.GetRawText(), options),
                    _ => null
                };
            }

            // Fallback for older versions
            if (root.TryGetProperty("MonthlySalary", out _))
                return JsonSerializer.Deserialize<FullTimeEmployee>(root.GetRawText(), options);
            if (root.TryGetProperty("HourlyRate", out _))
                return JsonSerializer.Deserialize<PartTimeEmployee>(root.GetRawText(), options);

            return null;
        }

        public override void Write(Utf8JsonWriter writer, Employee value, JsonSerializerOptions options)
        {
            var type = value switch
            {
                FullTimeEmployee => "FullTime",
                PartTimeEmployee => "PartTime",
                _ => "Employee"
            };

            using var jsonDoc = JsonDocument.Parse(JsonSerializer.Serialize(value, value.GetType(), options));

            writer.WriteStartObject();
            writer.WriteString("Type", type);

            foreach (var prop in jsonDoc.RootElement.EnumerateObject())
                prop.WriteTo(writer);

            writer.WriteEndObject();
        }
    }
}
