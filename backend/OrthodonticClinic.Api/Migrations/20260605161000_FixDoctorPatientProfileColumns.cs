using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrthodonticClinic.Api.Migrations
{
    public partial class FixDoctorPatientProfileColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvailabilityEnd",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailabilityStart",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvailableDays",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfileNote",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuardianEmail",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuardianFullName",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GuardianPhone",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [Doctors]
                SET [AvailabilityEnd] = N'15:00',
                    [AvailabilityStart] = N'08:00',
                    [AvailableDays] = N'Monday,Tuesday,Wednesday,Thursday',
                    [LicenseNumber] = N'ORTO-PL-1001',
                    [ProfileNote] = N'Specjalizuje się w leczeniu dzieci i młodzieży.'
                WHERE [Id] = 1;

                UPDATE [Doctors]
                SET [AvailabilityEnd] = N'18:00',
                    [AvailabilityStart] = N'10:00',
                    [AvailableDays] = N'Monday,Wednesday,Friday',
                    [LicenseNumber] = N'ORTO-PL-1002',
                    [ProfileNote] = N'Prowadzi skomplikowane przypadki ortodontyczne dorosłych.'
                WHERE [Id] = 2;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "AvailabilityEnd", table: "Doctors");
            migrationBuilder.DropColumn(name: "AvailabilityStart", table: "Doctors");
            migrationBuilder.DropColumn(name: "AvailableDays", table: "Doctors");
            migrationBuilder.DropColumn(name: "LicenseNumber", table: "Doctors");
            migrationBuilder.DropColumn(name: "ProfileNote", table: "Doctors");
            migrationBuilder.DropColumn(name: "GuardianEmail", table: "Patients");
            migrationBuilder.DropColumn(name: "GuardianFullName", table: "Patients");
            migrationBuilder.DropColumn(name: "GuardianPhone", table: "Patients");
        }
    }
}
