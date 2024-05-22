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
    ViewDayEndHistory,
    ViewTxPaymentHistory,
    ViewClockInOutHistory,
    ViewAccountInformation,
    ViewEwApiLog,
    
    // toolbox module
    ToolboxReleaseLicenseKey,
    ToolboxSuspendDataSync,
    ToolboxResetClientDataSync,
    ToolboxRollbackModifierFlow,
    ToolboxCopyMenuToNewAccount,
    ToolboxTrimDataSyncTracking,
    ToolboxSuspendPosUserAccounts,
    
    // settings module
    SettingsCheckForUpdates
}