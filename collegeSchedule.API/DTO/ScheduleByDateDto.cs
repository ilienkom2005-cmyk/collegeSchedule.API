namespace collegeSchedule.API.DTO
{
    public class ScheduleByDateDto
    {
        public DateTime LessonDate { get; set; }
        public string Weekday { get; set; } = null!;
        public List<LessonDto> Lessons { get; set; } = new(); // Список пар на этот день
    }
}
