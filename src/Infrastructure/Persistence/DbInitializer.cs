using Domain.Containers;
using Domain.ContainerTypes;
using Domain.Products;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.ProductTypes.AnyAsync()) return;

        // --- 1. ТИПИ ПРОДУКТІВ ---
        var dairy = ProductType.New("Молочна продукція", 7, null, null);
        var chemistry = ProductType.New("Побутова хімія", 365, null, null);
        var beverages = ProductType.New("Напої", 180, null, null);
        var grains = ProductType.New("Зернові", 730, null, null);
        context.ProductTypes.AddRange(dairy, chemistry, beverages, grains);
        await context.SaveChangesAsync();

        // --- 2. ТИПИ ТАРИ ---
        var glass = ContainerType.New("Скляна пляшка", "л", null, null);
        var plastic = ContainerType.New("Пластикова каністра", "л", null, null);
        var metal = ContainerType.New("Металева бочка", "л", null, null);
        var silo = ContainerType.New("Елеваторний силос", "кг", null, null);
        context.ContainerTypes.AddRange(glass, plastic, metal, silo);
        await context.SaveChangesAsync();

        // --- 3. ПРОДУКТИ ---
        var milk = Product.New("Молоко 2.5%", "Фермерське молоко", dairy.Id, null);
        var yogurt = Product.New("Йогурт Грецький", "Без цукру", dairy.Id, null);
        var bleach = Product.New("Відбілювач", "Хлорний", chemistry.Id, null);
        var water = Product.New("Вода мінеральна", "Газована", beverages.Id, null);
        var wheat = Product.New("Пшениця", "Вищий гатунок", grains.Id, null);
        context.Products.AddRange(milk, yogurt, bleach, water, wheat);
        await context.SaveChangesAsync();

        // --- 4. ТАРА ТА ОПЕРАЦІЇ (СЦЕНАРІЇ) ---
        var userId = Guid.Empty;

        // Сценарій 1: Порожня тара (щойно створена)
        var cEmpty = Container.New("EMPTY-01", "Нова бочка", 200m, "л", metal.Id, null, userId);
        context.Containers.Add(cEmpty);

        // Сценарій 2: Заповнена тара (свіжа)
        var cFull = Container.New("MILK-01", "Бідон 20л", 20m, "л", plastic.Id, null, userId);
        context.Containers.Add(cFull);
        await context.SaveChangesAsync();
        var fillFull = ContainerFill.New(cFull.Id, milk.Id, 18.5m, "л", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(6), userId);
        context.ContainerFills.Add(fillFull);
        await context.SaveChangesAsync();
        cFull.Fill(milk.Id, dairy.Id, 18.5m, "л", fillFull.ProductionDate, fillFull.ExpirationDate, fillFull.Id, userId);

        // Сценарій 3: Прострочена продукція (для ShowExpired=true)
        var cExpired = Container.New("YOG-EXP", "Контейнер йогурту", 5m, "кг", plastic.Id, null, userId);
        context.Containers.Add(cExpired);
        await context.SaveChangesAsync();
        var fillExp = ContainerFill.New(cExpired.Id, yogurt.Id, 4.8m, "кг", DateTime.UtcNow.AddDays(-10), DateTime.UtcNow.AddDays(-3), userId);
        context.ContainerFills.Add(fillExp);
        await context.SaveChangesAsync();
        cExpired.Fill(yogurt.Id, dairy.Id, 4.8m, "кг", fillExp.ProductionDate, fillExp.ExpirationDate, fillExp.Id, userId);

        // Сценарій 4: Тара, яку вже спорожнили (має LastProductId)
        var cWasFull = Container.New("CLEAN-01", "Каністра після хлору", 10m, "л", plastic.Id, null, userId);
        context.Containers.Add(cWasFull);
        await context.SaveChangesAsync();
        var fillOld = ContainerFill.New(cWasFull.Id, bleach.Id, 10m, "л", DateTime.UtcNow.AddDays(-20), DateTime.UtcNow.AddDays(300), userId);
        fillOld.Close(userId); // Спорожнена в історії
        context.ContainerFills.Add(fillOld);
        await context.SaveChangesAsync();
        // Емулюємо життєвий цикл: Fill -> Empty
        cWasFull.Fill(bleach.Id, chemistry.Id, 10m, "л", fillOld.ProductionDate, fillOld.ExpirationDate, fillOld.Id, userId);
        cWasFull.Empty(userId);

        // Сценарій 5: Тара з багатою історією (декілька наповнень)
        var cHistory = Container.New("HIST-01", "Багаторазова пляшка", 1m, "л", glass.Id, null, userId);
        context.Containers.Add(cHistory);
        await context.SaveChangesAsync();
        
        // Наповнення 1 (минуле)
        var f1 = ContainerFill.New(cHistory.Id, water.Id, 1m, "л", DateTime.UtcNow.AddDays(-30), DateTime.UtcNow.AddDays(150), userId);
        f1.Close(userId);
        // Наповнення 2 (поточне)
        var f2 = ContainerFill.New(cHistory.Id, water.Id, 0.9m, "л", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(178), userId);
        
        context.ContainerFills.AddRange(f1, f2);
        await context.SaveChangesAsync();
        cHistory.Fill(water.Id, beverages.Id, 0.9m, "л", f2.ProductionDate, f2.ExpirationDate, f2.Id, userId);

        // Сценарій 6: Велика тара (Силос)
        var cSilo = Container.New("SILO-01", "Силос №1", 10000m, "кг", silo.Id, null, userId);
        context.Containers.Add(cSilo);
        await context.SaveChangesAsync();
        var fillWheat = ContainerFill.New(cSilo.Id, wheat.Id, 8500m, "кг", DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(700), userId);
        context.ContainerFills.Add(fillWheat);
        await context.SaveChangesAsync();
        cSilo.Fill(wheat.Id, grains.Id, 8500m, "кг", fillWheat.ProductionDate, fillWheat.ExpirationDate, fillWheat.Id, userId);

        await context.SaveChangesAsync();
    }
}
