using EwAdminApi.Repositories.BaseClasses;
using EwAdminApi.Services;

namespace EwAdminApi.Repositories;

public class PosTableMasterRepository(
    IConnectionService connectionService,
    IHttpContextAccessor? httpContextAccessor = null) : PosRepositoryBase(connectionService, httpContextAccessor)
{
}