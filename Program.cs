using System;
using System.Collections.Generic;
using System.Linq;

namespace GymApp;

public interface IReportable
{
    string GetReport();
}

public abstract class GymEntity
{
    public string Id { get; set; }
    public string Name { get; set; }

    protected GymEntity(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public abstract string GetSummary();
}

public class Person : GymEntity
{
    public int Age { get; set; }

    public Person(string id, string name, int age) : base(id, name)
    {
        Age = age;
    }

    public virtual string GetRole() => "General Person";

    public override string GetSummary() => $"{GetRole()}: {Name} (ID: {Id}, Age: {Age})";
}

public class Membership
{
   

    public string Name { get; set; }
    public decimal MonthlyFee { get; set; }
    public int DurationMonths { get; set; }

    public Membership() : this("Standard", 40m, 1) { }

    public Membership(string name, decimal monthlyFee, int durationMonths)
    {
        Name = name;
        MonthlyFee = monthlyFee;
        DurationMonths = durationMonths;
    }

    public Membership(Membership other) : this(other.Name, other.MonthlyFee, other.DurationMonths) { }

    public decimal CalculateFee() => (MonthlyFee * DurationMonths) * (1);
    public decimal CalculateFee(decimal discount) => Math.Max(0, CalculateFee() - discount);

    public static string GenerateReceipt(string memberName, string packageName, decimal amountPaid)
    {
        return $"================ PAYMENT RECEIPT ================\n" +
               $" Member Name  : {memberName}\n" +
               $" Package      : {packageName}\n" +
               $" Amount Paid  : tk {amountPaid:F2}\n" +
               $" Date Issued  : {DateTime.Today:yyyy-MM-dd}\n" +
               $"================================================";
    }
}

public class WorkoutPlan
{
    public string PlanName { get; set; }
    public string Goal { get; set; }
    public List<string> Exercises { get; set; } = new();

    public WorkoutPlan(string planName, string goal)
    {
        PlanName = planName;
        Goal = goal;
    }

    public WorkoutPlan(WorkoutPlan other)
    {
        PlanName = other.PlanName;
        Goal = other.Goal;
        Exercises = new List<string>(other.Exercises);
    }

    public void AddExercise(string exercise) => Exercises.Add(exercise);
    public void AddExercise(string exercise, int sets, int reps) => Exercises.Add($"{exercise} ({sets} sets x {reps} reps)");

    public void UpdatePlan(string newGoal) => Goal = newGoal;
}

// 6. Trainer Class
public class Trainer : Person, IReportable
{
    public List<Member> AssignedMembers { get; } = new();

    public Trainer(string id, string name, int age) : base(id, name, age) { }
    public Trainer(string id, string name) : base(id, name, 30) { }

    public virtual int GetWorkload() => AssignedMembers.Count;

    public override string GetRole() => "Fitness Trainer";

    public string GetReport() => $"Trainer [{Name}] | Workload: {GetWorkload()} member(s) assigned.";
}

public class Member : Person, IReportable
{
    public Membership? CurrentMembership { get; private set; }
    public Trainer? AssignedTrainer { get; set; }
    public WorkoutPlan? Plan { get; set; }
    public DateTime ExpiryDate { get; private set; }
    public List<DateTime> AttendanceHistory { get; } = new();

    public Member() : this("M00", "Unknown Member", 18) { }
    public Member(string id, string name, int age) : base(id, name, age) { }

    public Member(Member other) : base(other.Id, other.Name, other.Age)
    {
        CurrentMembership = other.CurrentMembership != null ? new Membership(other.CurrentMembership) : null;
        Plan = other.Plan != null ? new WorkoutPlan(other.Plan) : null;
        AssignedTrainer = other.AssignedTrainer;
        ExpiryDate = other.ExpiryDate;
        AttendanceHistory = new List<DateTime>(other.AttendanceHistory);
    }

    public void AssignMembership(Membership membership)
    {
        CurrentMembership = membership;
        ExpiryDate = DateTime.Today.AddMonths(membership.DurationMonths);
    }

    public void RecordAttendance() => AttendanceHistory.Add(DateTime.Today);
    public void RecordAttendance(DateTime customDate) => AttendanceHistory.Add(customDate);

    public bool IsMembershipExpired() => DateTime.Today > ExpiryDate;

    public override string GetRole() => "Gym Member";

    public string GetReport()
    {
        string status = IsMembershipExpired() ? "Expired" : $"Active until {ExpiryDate:yyyy-MM-dd}";
        return $"Member [{Name}, ID: {Id}] | Status: {status} | Visits: {AttendanceHistory.Count} | Plan: {Plan?.PlanName ?? "None"}";
    }

    public static bool operator ==(Member? left, Member? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (left is null || right is null) return false;
        return left.Id == right.Id;
    }

    public static bool operator !=(Member? left, Member? right) => !(left == right);

    public override bool Equals(object? obj) => obj is Member other && this == other;
    public override int GetHashCode() => Id.GetHashCode();
}

public class Gym : IReportable
{
    public string GymName { get; set; }
    public List<Member> Members { get; } = new();
    public List<Trainer> Trainers { get; } = new();
    public List<Membership> MembershipPackages { get; } = new();
    public List<WorkoutPlan> Plans { get; } = new();

    public Gym(string gymName) => GymName = gymName;

    public void RegisterMember(Member member) => Members.Add(member);
    public void RemoveMember(string memberId) => Members.RemoveAll(m => m.Id == memberId);

    public void AddMembershipPackage(Membership pkg) => MembershipPackages.Add(pkg);

    public void AssignTrainer(Member member, Trainer trainer)
    {
        member.AssignedTrainer = trainer;
        if (!trainer.AssignedMembers.Contains(member))
        {
            trainer.AssignedMembers.Add(member);
        }
    }

    public void AddWorkoutPlan(WorkoutPlan plan) => Plans.Add(plan);

    public void DisplayMembershipStatuses()
    {
        Console.WriteLine("\n--- MEMBERSHIP STATUS TRACKER ---");
        foreach (var m in Members)
        {
            string state = m.IsMembershipExpired() ? "EXPIRED" : "ACTIVE";
            Console.WriteLine($"{m.Name} (ID: {m.Id}) -> {state} (Expires: {m.ExpiryDate:yyyy-MM-dd})");
        }
    }

    public void DisplayReports()
    {
        Console.WriteLine("\n--- TRAINER WORKLOAD REPORT ---");
        foreach (var t in Trainers) Console.WriteLine(t.GetReport());

        Console.WriteLine("\n--- MEMBER ACTIVITY REPORT ---");
        foreach (var m in Members) Console.WriteLine(m.GetReport());
    }

    public string GetReport() => $"{GymName} currently manages {Members.Count} members and {Trainers.Count} trainers.";
}

public static class Program
{
    public static void Main()
    {
        Gym gym = new("Apex Fitness Center");

        Membership basic = new("Basic Monthly", 30m, 1);
        Membership premium = new("Premium 6-Month", 70m, 6);
        gym.AddMembershipPackage(basic);
        gym.AddMembershipPackage(premium);

        WorkoutPlan weightLoss = new("Fat Shredder", "Weight Loss");
        weightLoss.AddExercise("Treadmill Running");                        
        weightLoss.AddExercise("Burpees", 4, 15);                            
        weightLoss.UpdatePlan("High-Intensity Interval Weight Loss");
        gym.AddWorkoutPlan(weightLoss);

        WorkoutPlan weightGain = new("Mass Builder", "Weight Gain");
        weightGain.AddExercise("Barbell Squats");                          
        weightGain.UpdatePlan("Hypertrophy Muscle Builder");
        gym.AddWorkoutPlan(weightGain);

        Trainer trainer1 = new("T1", "Lingkan", 21);
        Trainer trainer2 = new("T2", "Sabit", 21);               
        gym.Trainers.Add(trainer2);

        Member member1 = new("M1", "Koushik", 20);
        Member member2 = new("M2", "Israt", 35);
        Member member3 = new("M3", "Simila", 20); 
        Member member4 = new("M4", "Dinal Trump", 41); 
        gym.RegisterMember(member1);
        gym.RegisterMember(member2);
        gym.RegisterMember(member3);
        gym.RegisterMember(member4);

        member1.AssignMembership(premium);
        member2.AssignMembership(basic);
        member3.AssignMembership(basic);
        member4.AssignMembership(basic);

        gym.AssignTrainer(member1, trainer1);
        gym.AssignTrainer(member2, trainer1);
        gym.AssignTrainer(member3, trainer2);
        gym.AssignTrainer(member4, trainer1);


        member1.Plan = weightLoss;
        member2.Plan = weightGain;

        member1.RecordAttendance();
        member1.RecordAttendance(DateTime.Today.AddDays(-1));
        member2.RecordAttendance();

        decimal member1Fee = member1.CurrentMembership!.CalculateFee(20m); 
        Console.WriteLine(Membership.GenerateReceipt(member1.Name, member1.CurrentMembership.Name, member1Fee));

        gym.DisplayMembershipStatuses();
        gym.DisplayReports();

        Member clonedMember = new(member1);
        Console.WriteLine($"\nOperator Overload Equality Test: {member1 == clonedMember}");
    }
}
