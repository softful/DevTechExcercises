To handle the workload, it's decided to design coupon redemption table without any foreign keys or indexes.
Foreign keys could help to ensure the integrity of the data, but greatly reduce insertion performance on large tables.
To check if the user can redeem a coupon RedemptionCounter was introduced, which is an aggregated version of coupon redemption.
For the reporting purposes CouponRedemption table must be replicated to a separate database where indexes could be added to speed up the report.

