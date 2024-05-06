using Dapper;
using EwAdminApi.Models.Pos;
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
        using var db = await GetPosDatabaseConnection(accountId, shopId).ConfigureAwait(false);
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
    /// <returns></returns>
    public async Task<TxSalesHeader?> GetTxSalesHeaderAsync(int accountId, int shopId, int txSalesHeaderId)
    {
        using var db = await GetPosDatabaseConnection(accountId, shopId).ConfigureAwait(false);
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
    /// <returns></returns>
    public async Task<IEnumerable<TxSalesHeaderMin>?> GetTxSalesHeaderListAsync(int accountId, int shopId,
        DateTime txDate, int page, int pageSize)
    {
        using var db = await GetPosDatabaseConnection(accountId, shopId).ConfigureAwait(false);
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
}