using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using OrthodonticClinic.Api.Data;

#nullable disable

namespace OrthodonticClinic.Api.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260605161000_FixDoctorPatientProfileColumns")]
    partial class FixDoctorPatientProfileColumns
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);
        }
    }
}
