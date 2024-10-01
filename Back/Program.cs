using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<ReservationService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApiDbContext>(options => { options.UseSqlite(connectionString); });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/reservation", async (ReservationService reservations) => Results.Ok(await reservations.GetAll()));

app.MapGet("/reservation/{email}",
        async (ReservationService reservations, string email) => Results.Ok(await reservations.GetByEmail(email)));

app.MapPut("/reservation",
    async (ReservationService reservations, AddReservationDto dto) =>
    {
        var res = await reservations.AddReservation(dto);
        return res.Item1 ? Results.Ok(res.Item2) : Results.BadRequest(res.Item2);
    });

app.MapGet("/times/{year:int}-{month:int}-{day:int}",
    async (ReservationService reservations, int year, int month, int day) =>
        Results.Ok(await reservations.GetAvailableTimes(new DateOnly(year, month, day))));

app.MapGet("/get_dates", async (ReservationService reservations) => Results.Ok(await reservations.GetDates()));

app.MapGet("/get_instruments/{year:int}-{month:int}-{day:int}-{hour:int} ",
    async (ReservationService reservations, int year, int month, int day, int hour)
        => Results.Ok(await reservations.GetAvailableInstruments(new DateOnly(year, month, day), hour)));

app.Run();

internal partial class ReservationService
{
    [GeneratedRegex(@"^([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)@kiu\.edu\.ge$")]
    private static partial Regex KiuEmailRegex();

    private readonly ApiDbContext _db;

    public ReservationService(ApiDbContext db)
    {
        _db = db;
    }
    
    public async Task<List<Reservation>> GetByEmail(string email) => await _db.Reservations.Where(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase)).ToListAsync();
    public async Task<List<Reservation>> GetAll() => await _db.Reservations.ToListAsync();

    public async Task<List<DateOnly>> GetDates()
    {
        List<DateOnly> dates = [];
        var lastDate = DateOnly.FromDateTime(DateTime.Now);
        while (dates.Count < 7)
        {
            if ((await GetAvailableTimes(lastDate)).Count > 0)
            {
                dates.Add(lastDate);
            }

            lastDate = lastDate.AddDays(1);
        }

        return dates;
    }

    public async Task<Tuple<bool, string>> AddReservation(AddReservationDto dto)
    {
        if (!CheckEmail(dto.Email))
        {
            return Tuple.Create(false, "Wrong Email");
        }

        if (!GetAvailableTimes(DateOnly.FromDateTime(dto.Date)).Result.Contains(dto.Hour))
        {
            return Tuple.Create(false, "Wrong Hour");
        }

        var dateExists = await _db.Reservations.FirstOrDefaultAsync(x => x.Date == dto.Date.AddHours(dto.Hour));

        switch (dateExists)
        {
            case null:
            {
                await _db.Reservations.AddAsync(
                    new Reservation
                    {
                        Email = dto.Email,
                        Date = dto.Date.AddHours(dto.Hour),
                        IsBassTaken = dto.IsBassTaken,
                        IsMicrophoneTaken = dto.IsMicrophoneTaken,
                        IsOpen = dto.IsOpen,
                        IsGuitarTaken = dto.IsGuitarTaken,
                        IsDrumsTaken = dto.IsDrumsTaken,
                        IsPianoTaken = dto.IsPianoTaken
                    });
                await _db.SaveChangesAsync();

                return Tuple.Create(true, "Success");
            }

            case { IsOpen: false }:
                return Tuple.Create(false, "Reserved");

            case { IsOpen: true }:
            {
                dateExists.IsBassTaken = dto.IsBassTaken || dateExists.IsBassTaken;
                dateExists.IsMicrophoneTaken = dto.IsMicrophoneTaken || dateExists.IsMicrophoneTaken;
                dateExists.IsGuitarTaken = dto.IsGuitarTaken || dateExists.IsGuitarTaken;
                dateExists.IsDrumsTaken = dto.IsDrumsTaken || dateExists.IsDrumsTaken;
                dateExists.IsPianoTaken = dto.IsPianoTaken || dateExists.IsPianoTaken;
                await _db.SaveChangesAsync();
                return Tuple.Create(true, "Success");
            }
        }
    }

    public async Task<List<int>> GetAvailableTimes(DateOnly date)
    {
        var unavailable = await GetUnavailableTimes(date);

        List<int> available = date.DayOfWeek switch
        {
            DayOfWeek.Friday or DayOfWeek.Saturday => [15, 16, 17, 18, 19, 20, 21],
            _ => [17, 18, 19, 20]
        };
        
        available.RemoveAll(x => unavailable.Contains(x) || x <= DateTime.Now.Hour);
        return available;
    }

    public async Task<bool[]> GetAvailableInstruments(DateOnly date, int hour)
    {
        var res = await _db.Reservations.FirstOrDefaultAsync(x => x.Date == new DateTime(date, new TimeOnly(hour, 0)));
        return res == null
            ? [true, true, true, true, true]
            : [!res.IsGuitarTaken, !res.IsBassTaken, !res.IsDrumsTaken, !res.IsPianoTaken, !res.IsMicrophoneTaken];
    }

    private async Task<List<int>> GetUnavailableTimes(DateOnly date) =>
        await _db.Reservations
            .Where(res => DateOnly.FromDateTime(res.Date) == date && res.IsOpen == false)
            .Select(res => res.Date.TimeOfDay.Hours)
            .ToListAsync();

    private static bool CheckEmail(string email)
    {
        return KiuEmailRegex().Match(email).Success;
    }
}

internal class ApiDbContext : DbContext
{
    public virtual DbSet<Reservation> Reservations { get; init; }

    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }
}

internal class Reservation
{
    [Key] public int Id { get; init; }
    [StringLength(50)] public string Email { get; init; } = string.Empty;
    [DataType(DataType.DateTime)] public DateTime Date { get; init; }
    public bool IsGuitarTaken { get; set; }
    public bool IsBassTaken { get; set; }
    public bool IsDrumsTaken { get; set; }
    public bool IsPianoTaken { get; set; }
    public bool IsMicrophoneTaken { get; set; }
    public bool IsOpen { get; init; }
}

internal abstract class AddReservationDto
{
    public string Email { get; set; } = string.Empty;
    [DataType(DataType.Date)] public DateTime Date { get; set; }
    public int Hour { get; set; }
    public bool IsGuitarTaken { get; set; }
    public bool IsBassTaken { get; set; }
    public bool IsDrumsTaken { get; set; }
    public bool IsPianoTaken { get; set; }
    public bool IsMicrophoneTaken { get; set; }
    public bool IsOpen { get; set; }
}