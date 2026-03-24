using AurumSoftTask.Core.Models;
using AurumSoftTask.Services.Interfaces;
using AurumSoftTask.Services.Implementation;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text.Json;

namespace AurumSoftTask.Services.Tests.Implementation
{
    [TestFixture]
    public class JsonExportTests
    {
        [Test]
        public async Task ExportAsync_SavesDataToFile()
        {
            // Arrange
            var service = new JsonExportService();
            var data = new Well
            {
                WellId = "TestWell",
                X = 100.0,
                Y = 200.0,
                Intervals = new List<Interval>
                {
                    new Interval
                    {
                        DepthFrom = 0,
                        DepthTo = 50,
                        Rock = "TestRock1",
                        Porosity = 0.20
                    },
                    new Interval
                    {
                        DepthFrom = 50,
                        DepthTo = 100,
                        Rock = "TestRock2",
                        Porosity = 0.30
                    }
                }
            };
            string filePath = Path.GetTempFileName();

            // Act
            await service.ExportAsync(data, filePath);

            // Assert
            Assert.IsTrue(File.Exists(filePath));
            string fileContent = File.ReadAllText(filePath);
            string expectedContent = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            Assert.AreEqual(expectedContent, fileContent);

        }

        [Test]
        public async Task ExportAsync_SerializesCorrectly()
        {
            // Arrange
            var service = new JsonExportService();
            var data = new Well
            {
                WellId = "AnotherWell",
                X = 0,
                Y = 0,
                Intervals = new List<Interval>
                {
                    new Interval { DepthFrom = 10, DepthTo = 20, Rock = "RockA", Porosity = 0.25 }
                }
            };
            string filePath = Path.GetTempFileName();

            // Act
            await service.ExportAsync(data, filePath);

            // Assert
            string fileContent = File.ReadAllText(filePath);
            string expectedContent = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            Assert.AreEqual(expectedContent, fileContent);
        
        }
    }
}