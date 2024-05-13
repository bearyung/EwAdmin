using Dapper;
using EwAdmin.Common.Models.Pos;
using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosTxSalesRepository : PosRepositoryBase
{
    private readonly IConnectionService _connectionService;
    public PosTxSalesRepository(IConnectionService connectionService) : base(connectionService)
    {
        _connectionService = connectionService;
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
        using var db = await GetPosDatabaseConnection(accountId).ConfigureAwait(false);
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
            return await db.QuerySingleAsync<int>(query, parameters).ConfigureAwait(false);
        else
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
        using var db = await GetPosDatabaseConnection(accountId).ConfigureAwait(false);
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
            return await db.QuerySingleOrDefaultAsync<TxSalesHeader>(query, parameters).ConfigureAwait(false);
        else
            return null;
    }

    /// <summary>
    /// Returns a list of transactions for the given account, shop, and transaction date.
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="shopId"></param>
    /// <param name="txDate"></param>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <returns>
    /// A list of transactions for the given account, shop, and transaction date.
    /// </returns>
    public async Task<IEnumerable<TxSalesHeaderMin>?> GetTxSalesHeaderListAsync(int accountId, int shopId,
        DateTime txDate, int page, int pageSize)
    {
        using var db = await GetPosDatabaseConnection(accountId).ConfigureAwait(false);
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
            FROM [dbo].[TxSalesHeader]
            WHERE AccountId = @AccountId AND ShopId = @ShopId AND TxDate = @TxDate
            ORDER BY TxSalesHeaderId DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        var parameters = new
        {
            AccountId = accountId,
            ShopId = shopId,
            TxDate = txDate,
            Offset = offset,
            PageSize = pageSize
        };
        
        if (db != null)
        {
            return await db.QueryAsync<TxSalesHeader>(query, parameters).ConfigureAwait(false);
        }
        
        return null;
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
    public async Task<IEnumerable<TxPaymentMin>?> GetTxPaymentListAsync(int accountId, int shopId, int txSalesHeaderId, int page, int pageSize)
    {
        using var db = await GetPosDatabaseConnection(accountId).ConfigureAwait(false);
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
        
        return null;
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
        using var db = await GetPosDatabaseConnection(accountId).ConfigureAwait(false);
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
    /// Return true if the txpayment record is updated successfully
    /// </returns>
    public async Task<TxPayment?> UpdateTxPaymentAsync(TxPayment txPayment)
    {
        using var db = await GetPosDatabaseConnection(txPayment.AccountId).ConfigureAwait(false);
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
                return await GetTxPaymentAsync(txPayment.AccountId, txPayment.ShopId, txPayment.TxPaymentId).ConfigureAwait(false);
            }
        }

        return null;
    }
    
    
}