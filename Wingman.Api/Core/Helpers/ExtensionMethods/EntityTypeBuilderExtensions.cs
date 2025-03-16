using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using Wingman.Api.Core.Models;
using Wingman.Api.Features.Auth.Models;

namespace Wingman.Api.Core.Helpers.ExtensionMethods;

public static class EntityTypeBuilderExtensions
{
    public static void BaseConfiguration<T>(this EntityTypeBuilder<T> builder) where T : BaseModel
    {
        builder.HasKey(model => model.Id);
    }

    public static void CommonConfiguration<T>(this EntityTypeBuilder<T> builder) where T : CommonModel
    {
        builder.BaseConfiguration();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(model => model.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public static PropertyBuilder<TEnum> HasEnumCheckConstraint<T, TEnum>(this EntityTypeBuilder<T> builder,
                                                                          Expression<Func<T, TEnum>> propertyExpression)
        where TEnum : Enum where T : CommonModel
    {
        if (propertyExpression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException("Expression must be a property access expression", nameof(propertyExpression));
        }

        string propertyName = memberExpression.Member.Name;
        string tableName = builder.Metadata.GetTableName() ?? $"{typeof(T).Name}s";
        string constraintName = $"CHK_{tableName}_{propertyName}";

        string enumValues = string.Join(", ", Enum.GetValues(typeof(TEnum)).Cast<int>());
        string checkConstraint = $"\"{propertyName}\" IN ({enumValues})";

        builder.ToTable(table => table.HasCheckConstraint(constraintName, checkConstraint));

        return builder.Property(propertyExpression);
    }
}
