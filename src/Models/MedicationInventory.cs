using System.ComponentModel.DataAnnotations;

namespace HealthCareChallenge.Models;

public class MedicationInventory
{
    public int Id { get; set; }
    public string DrugName { get; set; }

    [ConcurrencyCheck]
    public int QuantityInStock { get; set; }
}

