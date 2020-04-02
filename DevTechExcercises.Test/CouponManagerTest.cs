using DevTechExcercises.Excercise_3;
using NUnit.Framework;
using Moq;
using System;
using System.Collections.Generic;

namespace DevTechExcercises.Test
{
    public class CouponManagerTest
    {
        [Test]
        public void CouponManagerCanRedeemCouponTest_ArgumentNull()
        {
            var logger = new Mock<ILogger>();
            var couponProvider = new Mock<ICouponProvider>();
            var couponManager = new CouponManager(logger.Object, couponProvider.Object);

            Assert.ThrowsAsync<ArgumentNullException>(async () => await couponManager.CanRedeemCoupon(Guid.Empty, Guid.Empty, null));
        }

        [Test]
        public void CouponManagerCanRedeemCouponTest_CouponNotFound()
        {
            var logger = new Mock<ILogger>();
            var couponProvider = new Mock<ICouponProvider>();
            var couponManager = new CouponManager(logger.Object, couponProvider.Object);
            var evals = new List<Func<Coupon, Guid, bool>>();

            Assert.ThrowsAsync<KeyNotFoundException>(async () => await couponManager.CanRedeemCoupon(Guid.Empty, Guid.Empty, evals));
        }

        [Test]
        public void CouponManagerCanRedeemCouponTest_EvaluatorsEmpty()
        {
            var logger = new Mock<ILogger>();
            var couponProvider = new Mock<ICouponProvider>();
            var couponManager = new CouponManager(logger.Object, couponProvider.Object);

            couponProvider.Setup(s => s.Retrieve(It.IsAny<Guid>())).ReturnsAsync(new Coupon());

            var evals = new List<Func<Coupon, Guid, bool>>();

            var result = couponManager.CanRedeemCoupon(Guid.Empty, Guid.Empty, evals).Result;

            Assert.IsTrue(result);
        }

        [Test]
        public void CouponManagerCanRedeemCouponTest_Evaluators()
        {
            var logger = new Mock<ILogger>();
            var couponProvider = new Mock<ICouponProvider>();
            var couponManager = new CouponManager(logger.Object, couponProvider.Object);

            couponProvider.Setup(s => s.Retrieve(It.IsAny<Guid>())).ReturnsAsync(new Coupon());

            var evals = new List<Func<Coupon, Guid, bool>>()
            {
                (coupon, guid) => false,
                (coupon, guid) => false
            };

            var result = couponManager.CanRedeemCoupon(Guid.Empty, Guid.Empty, evals).Result;
            
            Assert.IsFalse(result);

            evals = new List<Func<Coupon, Guid, bool>>()
            {
                (coupon, guid) => true,
                (coupon, guid) => false
            };

            var result1 = couponManager.CanRedeemCoupon(Guid.Empty, Guid.Empty, evals).Result;

            Assert.IsTrue(result1);
        }

        [Test]
        public void CouponManagerConstructorTest()
        {
            var logger = new Mock<ILogger>();
            var couponProvider = new Mock<ICouponProvider>();

            Assert.Throws<ArgumentNullException>(() => new CouponManager(logger.Object, null));
            Assert.Throws<ArgumentNullException>(() => new CouponManager(null, couponProvider.Object));
        }
    }
}