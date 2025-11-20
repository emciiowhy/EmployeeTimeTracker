using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmployeeTimeTracker.Models;

namespace EmployeeTimeTracker.Utilities
{
    public class PolymorphicEmployeeConverter : JsonConverter<Employee>
    {
        public override Employee Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var jsonDoc = JsonDocument.ParseValue(ref reader);
            var root = jsonDoc.RootElement;

            if (!root.TryGetProperty("Type", out JsonElement typeElement))
                throw new JsonException("Missing employee type discriminator.");

            string type = typeElement.GetString() ?? string.Empty;

            return type switch
            {
                "FullTimeEmployee" =>
                    JsonSerializer.Deserialize<FullTimeEmployee>(root.GetRawText(), options)
                    ?? throw new JsonException("Failed to deserialize FullTimeEmployee."),

                "PartTimeEmployee" =>
                    JsonSerializer.Deserialize<PartTimeEmployee>(root.GetRawText(), options)
                    ?? throw new JsonException("Failed to deserialize PartTimeEmployee."),

                _ => throw new JsonException($"Unknown employee type '{type}'.")
            };
        }

        public override void Write(Utf8JsonWriter writer, Employee value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("Type", value.GetType().Name);

            writer.WriteString("EmployeeId", value.EmployeeId);
            writer.WriteString("Name", value.Name);
            writer.WriteString("Email", value.Email);
            writer.WriteString("HireDate", value.HireDate.ToString("o"));

            switch (value)
            {
                case FullTimeEmployee ft:
                    writer.WriteNumber("MonthlySalary", ft.MonthlySalary);
                    writer.WriteNumber("OvertimeRate", ft.OvertimeRate);
                    break;

                case PartTimeEmployee pt:
                    writer.WriteNumber("HourlyRate", pt.HourlyRate);
                    break;
            }

            writer.WriteEndObject();
        }
    }
}
