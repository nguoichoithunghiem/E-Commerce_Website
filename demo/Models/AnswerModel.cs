namespace demo.Models
{
    public class AnswerModel
    {
        public long Id { get; set; }
        public long QuestionId { get; set; }
        public string Answer { get; set; }
        public string AdminName { get; set; }
        public DateTime DateAnswered { get; set; }

        // Navigation properties
        public QuestionModel Question { get; set; }
    }
}
