using HealthCareChallenge.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCareChallenge.Tests.Unit
{
    public class HealthCareChallenge
    {
        [Fact]
        public void MedicationInventory_QuantityInStock_HasConcurrencyCheckAttribute()
        {
            var property = typeof(MedicationInventory)
                .GetProperty(nameof(MedicationInventory.QuantityInStock));

            var hasConcurrencyCheck = property!
                .GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.ConcurrencyCheckAttribute), false)
                .Any();

            Assert.True(hasConcurrencyCheck);
        }
    }
}
