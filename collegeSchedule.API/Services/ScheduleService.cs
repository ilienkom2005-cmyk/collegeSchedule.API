using collegeSchedule.API.Data;
using collegeSchedule.API.DTO;
using collegeSchedule.API.Models;
using Microsoft.EntityFrameworkCore;

namespace collegeSchedule.API.Services
{
    public class ScheduleService:IScheduleService
    {
        private readonly AppDbContext _db;

        public ScheduleService(AppDbContext db) // Конструктор принимает DbContext через DI
        {
            _db = db;
        }

        public async Task<List<ScheduleByDateDto>> GetScheduleForGroup(string groupName, DateTime startDate, DateTime endDate)
        {
            //Валидация дат
            if (startDate > endDate)
                throw new ArgumentOutOfRangeException(nameof(startDate), "Дата начала больше даты окончания.");

            //Поиск группы по имени
            var group = await _db.StudentGroups
                                .FirstOrDefaultAsync(g => g.GroupName == groupName);
            if (group == null)
                throw new KeyNotFoundException($"Группа {groupName} не найдена.");

            //Загружаем расписание для группы в заданном диапазоне
            var schedules = await _db.Schedules
                                    .Where(s => s.GroupId == group.GroupId &&
                                                s.LessonDate >= startDate &&
                                                s.LessonDate <= endDate)
                                    .Include(s => s.Weekday)      // Загрузить день недели
                                    .Include(s => s.LessonTime)   // Загрузить время пары
                                    .Include(s => s.Subject)      // Загрузить предмет
                                    .Include(s => s.Teacher)      // Загрузить препода
                                    .Include(s => s.Classroom)    // Загрузить аудиторию
                                        .ThenInclude(c => c.Building) // Загрузить здание аудитории
                                    .OrderBy(s => s.LessonDate)   // Сортировка
                                    .ThenBy(s => s.LessonTime.LessonNumber)
                                    .ThenBy(s => s.GroupPart)
                                    .ToListAsync();

            var groupedByDate = schedules
                .GroupBy(s => s.LessonDate)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<ScheduleByDateDto>();

            // 5. Цикл по всем дням в диапазоне (включая дни без пар)
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Пропускаем воскресенье (день недели 0)
                if (date.DayOfWeek == DayOfWeek.Sunday)
                {
                    continue;
                }

                // Получить список пар для конкретного дня (или пустой список, если пар нет)
                var daySchedules = groupedByDate.GetValueOrDefault(date, new List<Schedule>());

                // Построить DTO для этого дня
                var dayDto = BuildDayDto(daySchedules, date);

                result.Add(dayDto);
            }

            return result;
        }

        // Вспомогательная функция для создания DTO одного дня
        // Принимает список расписаний для конкретного дня и саму дату
        private static ScheduleByDateDto BuildDayDto(List<Schedule> daySchedules, DateTime date)
        {
            // Группируем пары в этот день по номеру и времени
            var lessons = daySchedules
                .GroupBy(s => new { s.LessonTime.LessonNumber, s.LessonTime.TimeStart, s.LessonTime.TimeEnd })
                .Select(lessonGroup =>
                {
                    // Берем первую пару в группе для общих данных (если нет подгрупп, это вся группа)
                    var firstLessonInGroup = lessonGroup.First();
                    var lessonDto = new LessonDto
                    {
                        LessonNumber = firstLessonInGroup.LessonTime.LessonNumber,
                        Time = $"{firstLessonInGroup.LessonTime.TimeStart:HH:mm}-{firstLessonInGroup.LessonTime.TimeEnd:HH:mm}",
                        Subject = firstLessonInGroup.Subject.Name,
                        Teacher = $"{firstLessonInGroup.Teacher.LastName} {firstLessonInGroup.Teacher.FirstName} {firstLessonInGroup.Teacher.MiddleName ?? ""}".Trim(),
                        TeacherPosition = firstLessonInGroup.Teacher.Position,
                        Classroom = firstLessonInGroup.Classroom.RoomNumber,
                        Building = firstLessonInGroup.Classroom.Building.Name,
                        Address = firstLessonInGroup.Classroom.Building.Address,
                        GroupParts = new Dictionary<LessonGroupPart, LessonPartDto?>()
                    };

                    // Заполняем словарь GroupParts для каждой подгруппы (FULL, SUB1, SUB2), которая есть в расписании для этой пары
                    foreach (var part in lessonGroup)
                    {
                        lessonDto.GroupParts[part.GroupPart] = new LessonPartDto
                        {
                            Subject = part.Subject.Name,
                            Teacher = $"{part.Teacher.LastName} {part.Teacher.FirstName} {part.Teacher.MiddleName ?? ""}".Trim(),
                            TeacherPosition = part.Teacher.Position,
                            Classroom = part.Classroom.RoomNumber,
                            Building = part.Classroom.Building.Name,
                            Address = part.Classroom.Building.Address
                        };
                    }

                    // Если для этой пары не было расписания для всей группы (FULL), добавим null
                    if (!lessonDto.GroupParts.ContainsKey(LessonGroupPart.FULL))
                    {
                        lessonDto.GroupParts[LessonGroupPart.FULL] = null;
                    }
                    // То же самое для SUB1 и SUB2, если они не указаны, но нужны в UI
                    if (!lessonDto.GroupParts.ContainsKey(LessonGroupPart.SUB1))
                    {
                        lessonDto.GroupParts[LessonGroupPart.SUB1] = null;
                    }
                    if (!lessonDto.GroupParts.ContainsKey(LessonGroupPart.SUB2))
                    {
                        lessonDto.GroupParts[LessonGroupPart.SUB2] = null;
                    }

                    return lessonDto;
                })
                .OrderBy(l => l.LessonNumber) // Сортировка занятий по номеру внутри дня
                .ToList();

            return new ScheduleByDateDto
            {
                LessonDate = date, // Используем переданную дату
                Weekday = date.ToString("dddd", new System.Globalization.CultureInfo("ru-RU")), // Получаем имя дня недели на русском
                Lessons = lessons
            };
        }
    }
}
