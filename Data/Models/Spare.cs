namespace RestInventorySystem.Data.Models;

public class Spare : IModel, ICloneable
{
    public static string JsonFilePath { get; set; } = "spares.json"; // Default file path

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; }

    public string Description { get; set; }

    public string Company { get; set; }

    public decimal Price { get; set; }

    public int AvailableQuantity { get; set; }

    public static void DeleteSpare(List<Spare> spareList, Guid spareId)
    {
        Spare spareToRemove = spareList.FirstOrDefault(s => s.Id == spareId);
        if (spareToRemove != null)
        {
            spareList.Remove(spareToRemove);

            // Now, save the updated list of spares back to the JSON file
            SaveSpareListToJsonFile(spareList);
        }
    }

    private static void SaveSpareListToJsonFile(List<Spare> spareList)
    {
        string json = JsonSerializer.Serialize(spareList);

        // Save the JSON data to the file specified in JsonFilePath
        File.WriteAllText(JsonFilePath, json);
    }

    public object Clone()
    {
        return new Spare
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Company = Company,
            Price = Price,
            AvailableQuantity = AvailableQuantity
        };
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}
