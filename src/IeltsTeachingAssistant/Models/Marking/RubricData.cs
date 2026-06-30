using System.Collections.Generic;

namespace IeltsTeachingAssistant.Models.Marking
{
    public class RubricData
    {
        public List<string> Criteria { get; set; } = new();
        public List<int> Bands { get; set; } = new();
        public List<RubricCellData> Cells { get; set; } = new();
    }

    public class RubricCellData
    {
        public int Band { get; set; }
        public string Criteria { get; set; } = string.Empty;
        public List<string> Descriptors { get; set; } = new();
    }
}
