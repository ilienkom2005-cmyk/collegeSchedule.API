using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace collegeSchedule.API.Models
{
    [Table("lesson_time")]
    public class LessonTime
    {
        [Key]
        [Column("lesson_time_id")]
        public int LessonTimeID { get; set; }

        [Column("lesson_number")]
        public int LessonNumber { get; set; }

        [Column("time_start")]
        public TimeOnly TimeStart { get; set; }

        [Column("time_end")]
        public TimeOnly TimeEnd { get; set; }
    }
}
