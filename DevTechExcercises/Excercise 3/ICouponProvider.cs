using System;
using System.Threading.Tasks;

namespace DevTechExcercises.Excercise_3
{
    public interface ICouponProvider
    {
        Task<Coupon> Retrieve(Guid couponId);
    }
}