using DriveNow.Domain.Entities;

namespace DriveNow.Application.Abstractions.Repositories;

public interface IPromoCodeRepository
{
    PromoCode? GetByCode(string code);
    void Update(PromoCode promo);
}