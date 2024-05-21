namespace EwAdminApp.Models;

public enum UserFunctionEnum
{
    // fix data module
    FixWorkdayDetail,
    FixWorkdayPeriodDetail,
    FixTxPayment,
    FixItemCategory,
    FixIncorrectDayEnd,
    FixTableMaster,
    FixTxTableRemap,
    FixTxCashDrawerRemap,
    FixTxCustomerCount,
    // view data module
    
    // toolbox module
    ToolboxReleaseLicenseKey,
    ToolboxSuspendDataSync,
    ToolboxResetClientDataSync,
    ToolboxRollbackModifierFlow,
    ToolboxCopyMenuToNewAccount,
    ToolboxTrimDataSyncTracking,
    ToolboxSuspendPosUserAccounts,
    ViewDayEndHistory,
    ViewTxPaymentHistory,
    ViewClockInOutHistory,
    ViewAccountInformation,
    ViewEwApiLog
}