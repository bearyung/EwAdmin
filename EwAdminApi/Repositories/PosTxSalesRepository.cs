using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosTxSalesRepository : PosTxSalesRepositoryBase
{
    public PosTxSalesRepository(IConnectionService connectionService, IHttpContextAccessor httpContextAccessor) :
        base(connectionService, httpContextAccessor)
    {
    }

    /// <summary>
    /// Check if there are any transactions in the given workday detail id
    /// returns the count of transactions
    /// return -1 if there is an error
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="workdayDetailId"></param>
    /// <returns>An integer representing the count of transactions for the given workday detail id, or -1 if there is an error.</returns>
    public async Task<int> GetTxCountInWorkdayDetailIdAsync(int accountId, int shopId, int workdayDetailId)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var query = @"
            select count(1) from TxSalesHeader a 
            where a.accountid = @AccountId and a.shopid = @ShopId
            and (
                WorkdayPeriodDetailId in
                    (select WorkdayPeriodDetailId from ShopWorkdayPeriodDetail where AccountId = a.AccountId and ShopId = a.ShopId and WorkdayDetailId = @WorkdayDetailId)
                or StartWorkdayPeriodDetailId in 
                    (select WorkdayPeriodDetailId from ShopWorkdayPeriodDetail where AccountId = a.AccountId and ShopId = a.ShopId and WorkdayDetailId = @WorkdayDetailId)
            )";
        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            WorkdayDetailId = workdayDetailId
        };
        if (db != null)
        {
            return await db.QuerySingleAsync<int>(query, parameters).ConfigureAwait(false);
        }

        return -1;
    }

    /// <summary>
    /// Returns the details of a transaction given the txsalesheaderid, accountid, and shopid.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="txSalesHeaderId"></param>
    /// <returns>
    /// A transaction details object for the given txsalesheaderid, accountid, and shopid.
    /// </returns>
    public async Task<TxSalesHeader?> GetTxSalesHeaderAsync(int accountId, int shopId, int txSalesHeaderId)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var query = @"
            SELECT 
                [TxSalesHeaderId]
              ,[AccountId]
              ,[ShopId]
              ,[TxCode]
              ,[TxDate]
              ,[ReceiptNo]
              ,[IsCurrentTx]
              ,[Voided]
              ,[Enabled]
              ,[TableId]
              ,[TableCode]
              ,[PreviousTableId]
              ,[PreviousTableCode]
              ,[SectionId]
              ,[SectionName]
              ,[CheckinDatetime]
              ,[CheckoutDatetime]
              ,[CheckinUserId]
              ,[CheckinUserName]
              ,[CheckoutUserId]
              ,[CheckoutUserName]
              ,[CashierUserId]
              ,[CashierUserName]
              ,[CashierDatetime]
              ,[AmountPaid]
              ,[AmountChange]
              ,[AmountSubtotal]
              ,[AmountServiceCharge]
              ,[AmountDiscount]
              ,[AmountTotal]
              ,[AmountRounding]
              ,[TxCompleted]
              ,[TxChecked]
              ,[CreatedDate]
              ,[CreatedBy]
              ,[ModifiedDate]
              ,[ModifiedBy]
              ,[IsTakeAway]
              ,[TakeAwayRunningIndex]
              ,[DisabledReasonId]
              ,[DisabledReasonDesc]
              ,[DisabledByUserId]
              ,[DisabledByUserName]
              ,[DisabledDateTime]
              ,[WorkdayPeriodDetailId]
              ,[WorkdayPeriodName]
              ,[DiscountId]
              ,[DiscountName]
              ,[CashDrawerCode]
              ,[ReceiptPrintCount]
              ,[TxRevokeCount]
              ,[ServiceChargeId]
              ,[ServiceChargeName]
              ,[AmountTips]
              ,[IsTimeLimited]
              ,[TimeLimitedMinutes]
              ,[CusCount]
              ,[DiscountByUserId]
              ,[DiscountByUserName]
              ,[AmountPointTotal]
              ,[MemberPointRemain]
              ,[TaxationId]
              ,[TaxationName]
              ,[AmountTaxation]
              ,[AmountMinChargeOffset]
              ,[IsMinChargeOffsetWaived]
              ,[IsMinChargeTx]
              ,[IsMinChargePerHead]
              ,[MinChargeAmount]
              ,[MinChargeMemberAmount]
              ,[IsPrepaidRechargeTx]
              ,[IsInvoicePrintPending]
              ,[InvoiceNum]
              ,[OrderNum]
              ,[IsDepositTx]
              ,[TotalDepositAmount]
              ,[DepositRemark]
              ,[IsDepositOutstanding]
              ,[IsReturnTx]
              ,[HasReturned]
              ,[ReturnedDateTime]
              ,[ReturnedTxSalesHeaderId]
              ,[NewTxSalesHeaderIdForReturn]
              ,[ApiGatewayRefId]
              ,[ApiGatewayName]
              ,[ApiGatewayRefRemark]
              ,[TableRemark]
              ,[TxSalesHeaderRemark]
              ,[TotalPaymentMethodSurchargeAmount]
              ,[IsNonSalesTx]
              ,[IsNoOtherLoyaltyTx]
              ,[StartWorkdayPeriodDetailId]
              ,[StartWorkdayPeriodName]
              ,[IsTxOnHold]
              ,[IsOdoTx]
              ,[OdoOrderToken]
              ,[AmountOverpayment]
              ,[TxStatusId]
              ,[OverridedChecklistPrinterName]
              ,[OrderSourceTypeId]
              ,[OrderSourceRefId]
              ,[OrderChannelId]
              ,[OrderChannelCode]
              ,[OrderChannelName]
              ,[ApiGatewayRefCode]
              ,[ApiGatewayResponseCode]
            FROM [dbo].[TxSalesHeader]
            WHERE TxSalesHeaderId = @TxSalesHeaderId AND AccountId = @AccountId AND ShopId = @ShopId";
        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            TxSalesHeaderId = txSalesHeaderId
        };
        if (db != null)
        {
            return await db.QuerySingleOrDefaultAsync<TxSalesHeader>(query, parameters).ConfigureAwait(false);
        }

        return null;
    }

    /// <summary>
    /// Returns a list of transactions for the given account, shop, and transaction date range.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="txDateGte"></param>
    /// <param name="txDateLte"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="txSalesHeaderId"></param>
    /// <param name="tableCode"></param>
    /// <param name="cusCountGte"></param>
    /// <param name="amountTotalGte"></param>
    /// <param name="amountTotalLte"></param>
    /// <returns>
    /// A list of transactions for the given account, shop, and transaction date range.
    /// </returns>
    public async Task<IEnumerable<TxSalesHeaderMin>> GetTxSalesHeaderListAsync(int accountId, int shopId,
        DateTime? txDateGte, DateTime? txDateLte, int page, int pageSize,
        int? txSalesHeaderId = null, string? tableCode = null,
        int? cusCountGte = null, decimal? amountTotalGte = null, decimal? amountTotalLte = null)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var offset = (page - 1) * pageSize;

        var query = @"
        SELECT 
            [TxSalesHeaderId]
          ,[AccountId]
          ,[ShopId]
          ,[TxCode]
          ,[TxDate]
          ,[Enabled]
          ,[TableId]
          ,[TableCode]
          ,[CheckinDatetime]
          ,[CheckoutDatetime]
          ,[CheckinUserId]
          ,[CheckinUserName]
          ,[CheckoutUserId]
          ,[CheckoutUserName]
          ,[CashierUserId]
          ,[CashierUserName]
          ,[CashierDatetime]
          ,[AmountPaid]
          ,[AmountChange]
          ,[AmountSubtotal]
          ,[AmountServiceCharge]
          ,[AmountDiscount]
          ,[AmountTotal]
          ,[AmountRounding]
          ,[TxCompleted]
          ,[TxChecked]
          ,[AmountTaxation]
          ,[AmountMinChargeOffset]
          ,[DisabledReasonId]
          ,[DisabledReasonDesc]
          ,[DisabledByUserId]
          ,[DisabledByUserName]
          ,[DisabledDateTime]
          ,[IsodoTx]
          ,[CashDrawerCode]
        FROM [dbo].[TxSalesHeader]
        WHERE AccountId = @AccountId AND ShopId = @ShopId
        AND (@TxSalesHeaderId IS NULL OR TxSalesHeaderId = @TxSalesHeaderId)
        AND (@TableCode IS NULL OR TableCode = @TableCode)
        AND (@CusCountGte IS NULL OR CusCount >= @CusCountGte)
        AND (@AmountTotalGte IS NULL OR AmountTotal >= @AmountTotalGte)
        AND (@AmountTotalLte IS NULL OR AmountTotal <= @AmountTotalLte)
        AND (@TxDateGte IS NULL OR TxDate >= @TxDateGte)
        AND (@TxDateLte IS NULL OR TxDate <= @TxDateLte)
        ORDER BY TxSalesHeaderId DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            TxDateGte = txDateGte,
            TxDateLte = txDateLte,
            TxSalesHeaderId = txSalesHeaderId,
            Offset = offset,
            PageSize = pageSize,
            TableCode = tableCode,
            CusCountGte = cusCountGte,
            AmountTotalGte = amountTotalGte,
            AmountTotalLte = amountTotalLte
        };

        if (db != null)
        {
            return await db.QueryAsync<TxSalesHeaderMin>(query, parameters).ConfigureAwait(false);
        }

        return [];
    }


    /// <summary>
    /// Returns a list of payments for the given account, shop, and transaction sales header id.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="txSalesHeaderId"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns>
    /// A list of payments for the given account, shop, and transaction sales header id.
    /// </returns>
    public async Task<IEnumerable<TxPaymentMin>> GetTxPaymentListAsync(int accountId, int shopId, int txSalesHeaderId,
        int page, int pageSize)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var offset = (page - 1) * pageSize;
        var query = @"
            SELECT 
                a.[TxPaymentId]
                ,a.[AccountId]
                ,a.[ShopId]
                ,a.[TxSalesHeaderId]
                ,a.[PaymentMethodId]
                ,a.[TotalAmount]
                ,a.[PaidAmount]
                ,a.[Enabled]
                ,b.[PaymentMethodCode]
                ,b.[PaymentMethodName]
            FROM [dbo].[TxPayment] a
            LEFT JOIN [dbo].[PaymentMethod] b
            ON a.AccountId = b.AccountId and a.PaymentMethodId = b.PaymentMethodId
            WHERE a.AccountId = @AccountId AND a.ShopId = @ShopId AND a.TxSalesHeaderId = @TxSalesHeaderId
            ORDER BY TxPaymentId DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            TxSalesHeaderId = txSalesHeaderId,
            Offset = offset,
            PageSize = pageSize
        };

        if (db != null)
        {
            return await db.QueryAsync<TxPaymentMin>(query, parameters).ConfigureAwait(false);
        }

        return [];
    }

    // A new method to get the full details (all the fields in TxPayment class TxPayment : TxPaymentMin) of txpayment given the txpaymentid
    // input: accountId, shopId, txPaymentId
    // output: TxPayment object 
    /// <summary>
    /// Return the full details of the txpayment object for the given txpaymentid, accountid, and shopid
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="txPaymentId"></param>
    /// <returns>
    /// Return the full details of the txpayment object
    /// - If the txpayment object is found, return the txpayment object
    /// - If the txpayment object is not found, return null
    /// </returns>
    public async Task<TxPayment?> GetTxPaymentAsync(int accountId, int shopId, int txPaymentId)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var query = @"
            SELECT 
                a.[TxPaymentId]
                ,a.[AccountId]
                ,a.[ShopId]
                ,a.[TxSalesHeaderId]
                ,a.[PaymentMethodId]
                ,a.[TotalAmount]
                ,a.[PaidAmount]
                ,a.[Enabled]
                ,a.[CreatedDate]
                ,a.[CreatedBy]
                ,a.[ModifiedDate]
                ,a.[ModifiedBy]
                ,a.[OclNum]
                ,a.[OclRemainValue]
                ,a.[OclDeviceNum]
                ,a.[RefNum]
                ,a.[Remark1]
                ,a.[Remark2]
                ,a.[Remark3]
                ,a.[Remark4]
                ,a.[Remark5]
                ,a.[Remark6]
                ,a.[Remark7]
                ,a.[Remark8]
                ,a.[Remark9]
                ,a.[Remark10]
                ,a.[ChangeAmount]
                ,a.[CashoutAmount]
                ,a.[TipAmount]
                ,a.[IsDepositPayment]
                ,a.[DepositReceivedByUserId]
                ,a.[DepositReceivedByUserName]
                ,a.[DepositReceivedDatetime]
                ,a.[DepositWorkdayDetailId]
                ,a.[DepositWorkdayPeriodDetailId]
                ,a.[DepositWorkdayPeriodName]
                ,a.[PaymentMethodSurchargeAmount]
                ,a.[PaymentMethodSurchargeRate]
                ,a.[IsNonSalesTxPayment]
                ,a.[OverpaymentAmount]
                ,a.[IsPreprintedCouponTxPayment]
                ,a.[PaymentRemark]
                ,a.[PaymentCurrency]
                ,a.[PaymentPathway]
                ,a.[PaidAmountFx]
                ,a.[ChangeAmountFx]
                ,a.[IsFxPayment]
                ,a.[IsChangeAmountFx]
                ,a.[PaymentFxRate]
                ,a.[TotalAmountFx]
                ,b.[PaymentMethodCode]
                ,b.[PaymentMethodName]
                ,a.[TxChargesRate]
                ,a.[TxTotalCharges]
                ,a.[TxTipCharges]
                ,a.[TxNetTotal]
                ,a.[TxNetTip]
                ,a.[IsOdoTx]
                ,a.[IsOnlinePayment]
            FROM [dbo].[TxPayment] a
            LEFT JOIN [dbo].[PaymentMethod] b
            ON a.AccountId = b.AccountId and a.PaymentMethodId = b.PaymentMethodId
            WHERE a.AccountId = @AccountId AND a.ShopId = @ShopId AND a.TxPaymentId = @TxPaymentId";
        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            TxPaymentId = txPaymentId
        };

        if (db != null)
        {
            return await db.QuerySingleOrDefaultAsync<TxPayment>(query, parameters).ConfigureAwait(false);
        }

        return null;
    }

    // a new method to update the txpayment record
    // input: txPayment object
    //        Only the PaymentMethodId, Enabled and ModifiedBy can be updated, modifiedDate is updated automatically
    // output: boolean
    /// <summary>
    /// Update the txpayment record
    /// </summary>
    /// <param name="txPayment"></param>
    /// <returns>
    /// Return true if the txPayment record is updated successfully
    /// </returns>
    public async Task<TxPayment?> UpdateTxPaymentAsync(TxPayment txPayment)
    {
        using var db = await GetPosDatabaseConnectionByAccount(txPayment.AccountId).ConfigureAwait(false);
        var query = @"
            UPDATE [dbo].[TxPayment]
            SET PaymentMethodId = @PaymentMethodId, Enabled = @Enabled, ModifiedBy = @ModifiedBy, ModifiedDate = GETDATE()
            WHERE AccountId = @AccountId AND ShopId = @ShopId AND TxPaymentId = @TxPaymentId";
        var parameters = new
        {
            txPayment.AccountId,
            txPayment.ShopId,
            txPayment.TxPaymentId,
            txPayment.PaymentMethodId,
            txPayment.Enabled,
            txPayment.ModifiedBy
        };

        if (db != null)
        {
            // if update is successful, return the updated txpayment object
            // otherwise, return null
            var result = await db.ExecuteAsync(query, parameters).ConfigureAwait(false);
            if (result > 0)
            {
                return await GetTxPaymentAsync(txPayment.AccountId, txPayment.ShopId, txPayment.TxPaymentId)
                    .ConfigureAwait(false);
            }
        }

        return null;
    }

    // add a new method to update the txSalesHeader 
    // input: txSalesHeader object
    //        Only the CusCount, TableId, TableCode, SectionId, SectionName, Enabled and ModifiedBy can be updated, modifiedDate is updated automatically
    // output: boolean
    public async Task<TxSalesHeader?> UpdateTxSalesHeaderAsync(TxSalesHeader txSalesHeaderObj)
    {
        using var db = await GetPosDatabaseConnectionByAccount(txSalesHeaderObj.AccountId).ConfigureAwait(false);
        var (query, parameters) = BuildTxSalesHeaderUpdateQuery(txSalesHeaderObj, [
            nameof(TxSalesHeader.CusCount),
            nameof(TxSalesHeader.TableId),
            nameof(TxSalesHeader.TableCode),
            nameof(TxSalesHeader.SectionId),
            nameof(TxSalesHeader.SectionName),
            nameof(TxSalesHeader.Enabled)
        ], [
            nameof(TxSalesHeader.ModifiedDate),
            nameof(TxSalesHeader.ModifiedBy)
        ]);


        if (db != null)
        {
            // if update is successful, return the updated txsalesheader object
            // otherwise, return null
            var result = await db.ExecuteAsync(query, parameters).ConfigureAwait(false);
            if (result > 0)
            {
                return await GetTxSalesHeaderAsync(txSalesHeaderObj.AccountId, txSalesHeaderObj.ShopId,
                        txSalesHeaderObj.TxSalesHeaderId)
                    .ConfigureAwait(false);
            }
        }

        return null;
    }
    
    // add a new method to check if there's any closed transaction
    // do not have the CashDrawerCode
    // input: accountId, shopId
    // output: int (count of closed transactions)
    public async Task<int> GetClosedTxCountWithoutCashDrawerCodeAsync(int accountId, int shopId)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var query = @"
            SELECT COUNT(1) FROM TxSalesHeader
            WHERE AccountId = @AccountId AND ShopId = @ShopId
            AND Enabled = 1 AND TxCompleted = 1 AND TxChecked = 1 
            AND AmountTotal <> 0
            AND CashDrawerCode IS NULL";
        
        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId
        };

        if (db != null)
        {
            return await db.QuerySingleAsync<int>(query, parameters).ConfigureAwait(false);
        }

        return -1;
    }
    
    // add a new method to get the closed transactions
    // do not have the CashDrawerCode
    // input: accountId, shopId, page, pageSize
    // output: List of TxSalesHeaderMin
    public async Task<IEnumerable<TxSalesHeaderMin>> GetClosedTxWithoutCashDrawerCodeAsync(int accountId, int shopId, int page, int pageSize)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var offset = (page - 1) * pageSize;
        var query = @"
            SELECT 
                [TxSalesHeaderId]
              ,[AccountId]
              ,[ShopId]
              ,[TxCode]
              ,[TxDate]
              ,[Enabled]
              ,[TableId]
              ,[TableCode]
              ,[CheckinDatetime]
              ,[CheckoutDatetime]
              ,[CheckinUserId]
              ,[CheckinUserName]
              ,[CheckoutUserId]
              ,[CheckoutUserName]
              ,[CashierUserId]
              ,[CashierUserName]
              ,[CashierDatetime]
              ,[AmountPaid]
              ,[AmountChange]
              ,[AmountSubtotal]
              ,[AmountServiceCharge]
              ,[AmountDiscount]
              ,[AmountTotal]
              ,[AmountRounding]
              ,[TxCompleted]
              ,[TxChecked]
              ,[AmountTaxation]
              ,[AmountMinChargeOffset]
              ,[DisabledReasonId]
              ,[DisabledReasonDesc]
              ,[DisabledByUserId]
              ,[DisabledByUserName]
              ,[DisabledDateTime]
              ,[IsodoTx]
              ,[CashDrawerCode]
            FROM [dbo].[TxSalesHeader]
            WHERE AccountId = @AccountId AND ShopId = @ShopId
            AND Enabled = 1 AND TxCompleted = 1 AND TxChecked = 1 
            AND AmountTotal <> 0
            AND CashDrawerCode IS NULL
            ORDER BY TxSalesHeaderId DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            Offset = offset,
            PageSize = pageSize
        };

        if (db != null)
        {
            return await db.QueryAsync<TxSalesHeaderMin>(query, parameters).ConfigureAwait(false);
        }

        return [];
    }
    
    // add a new method to update the CashDrawerCode for the closed transactions
    // do not have the CashDrawerCode
    // input: accountId, shopId, cashDrawerCode
    // output: int (count of updated transactions)
    public async Task<int> UpdateCashDrawerCodeForClosedTxWithoutCashDrawerCodeAsync(int accountId, int shopId, string cashDrawerCode)
    {
        using var db = await GetPosDatabaseConnectionByAccount(accountId).ConfigureAwait(false);
        var query = @"
            UPDATE TxSalesHeader
            SET CashDrawerCode = @CashDrawerCode
            WHERE AccountId = @AccountId AND ShopId = @ShopId
            AND Enabled = 1 AND TxCompleted = 1 AND TxChecked = 1 
            AND AmountTotal <> 0
            AND CashDrawerCode IS NULL";
        
        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            CashDrawerCode = cashDrawerCode
        };

        if (db != null)
        {
            return await db.ExecuteAsync(query, parameters).ConfigureAwait(false);
        }

        return -1;
    }
    
}