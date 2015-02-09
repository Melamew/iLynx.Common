using System;
using System.Collections.Generic;
using System.Linq;
using iLynx.Chatter.Infrastructure;
using iLynx.Chatter.Infrastructure.Domain;
using iLynx.Chatter.Infrastructure.Services;
using iLynx.Common;
using iLynx.Common.DataAccess;

namespace iLynx.Chatter.ServerServicesModule
{
    public class UserPermissionService : IUserPermissionService
    {
        private readonly IDataAdapter<Permission> permissionsAdapter;
        private readonly IConsoleHandler consoleHandler;
        private readonly IDataAdapter<User> userAdapter;

        public UserPermissionService(IDataAdapter<User> userAdapter,
            IDataAdapter<Permission> permissionsAdapter,
            IConsoleHandler consoleHandler,
            ICommandHandlerRegistry commandHandlerRegistry)
        {
            this.permissionsAdapter = Guard.IsNull(() => permissionsAdapter);
            this.consoleHandler = Guard.IsNull(() => consoleHandler);
            this.userAdapter = Guard.IsNull(() => userAdapter);
            this.consoleHandler.RegisterCommand("permissions", x => commandHandlerRegistry.Execute(x[0], x.Skip(1).ToArray()), "", commandHandlerRegistry.SuggestAutoComplete);
            commandHandlerRegistry.RegisterCommand("list", OnListPermissions, "Lists permissions for the specified user. list [username]");
            commandHandlerRegistry.RegisterCommand("grant", OnGrantPermission, "Grants the specified user the specified permission. grant {username} {permission}", (s, strings) => ListPermissions(s, OnGrantPermission));
            commandHandlerRegistry.RegisterCommand("deny", OnDenyPermission, "Denies the specified user the specified permission. deny {username} {permission}", (s, strings) => ListPermissions(s, OnDenyPermission));
        }

        private CommandDefinition[] ListPermissions(string s, Action<string[]> callback)
        {
            return permissionsAdapter.Query().Where(x => x.PermissionIdentifier.StartsWith(s)).Select(x => new CommandDefinition
            {
                Callback = OnGrantPermission,
                Command = s + " " + x.PermissionIdentifier,
                HelpText = "",
            }).ToArray();
        }

        private bool TryParse(string[] arguments, out User user, out Permission permission, bool createMissingPermission = false)
        {
            permission = null;
            var userName = arguments.FirstOrDefault();
            var permissionIdentifier = arguments.Skip(1).FirstOrDefault();
            user = userAdapter.GetFirst(x => x.Username == userName);
            if (null == user)
            {
                consoleHandler.WriteLine("Could not find user: {0}", userName);
                return false;
            }
            permission = permissionsAdapter.GetFirst(x => x.PermissionIdentifier == permissionIdentifier);
            if (null != permission || !createMissingPermission) return true;
            permission = new Permission { PermissionIdentifier = permissionIdentifier };
            permissionsAdapter.SaveOrUpdate(permission);
            return true;
        }

        private void OnDenyPermission(string[] obj)
        {
            User user;
            Permission permission;
            if (!TryParse(obj, out user, out permission)) return;
            if (null == permission)
            {
                consoleHandler.WriteLine("User {0} does not have the specified permission", user.Username);
                return;
            }
            user.Permissions.Remove(permission);
            userAdapter.SaveOrUpdate(user);
            consoleHandler.WriteLine("Removed permission {0} from user {1}", permission.PermissionIdentifier, user.Username);
        }

        private void OnGrantPermission(string[] obj)
        {
            User user;
            Permission permission;
            if (!TryParse(obj, out user, out permission, true)) return;
            if (null == user.Permissions)
                user.Permissions = new List<Permission>();
            user.Permissions.Add(permission);
            userAdapter.SaveOrUpdate(user);
            consoleHandler.WriteLine("Granted {0} to {1}", permission.PermissionIdentifier, user.Username);
        }

        private void OnListPermissions(string[] strings)
        {
            var userName = strings.FirstOrDefault();
            if (string.IsNullOrEmpty(userName))
            {
                ListAllPermissions();
                return;
            }
            var user = userAdapter.GetFirst(x => x.Username == userName);
            if (null == user)
            {
                consoleHandler.WriteLine("Unknown user: {0}", userName);
                return;
            }
            if (null == user.Permissions || user.Permissions.Count < 1)
            {
                consoleHandler.WriteLine("User {0} has no permissions", userName);
                return;
            }
            consoleHandler.WriteLine("User {0} has the following permissions", userName);
            foreach (var permission in user.Permissions)
                consoleHandler.WriteLine("  {0}", permission.PermissionIdentifier);
        }

        private void ListAllPermissions()
        {
            foreach (var permission in permissionsAdapter.Query())
                consoleHandler.WriteLine("  {0}", permission.PermissionIdentifier);
        }

        public bool HasPermission(User user, string permissionIdentifier)
        {
            return null != user.Permissions && user.Permissions.Any(x => x.PermissionIdentifier == permissionIdentifier);
        }

        public void CreatePermission(string permissionIdentifier)
        {
            var existing = permissionsAdapter.GetFirst(x => x.PermissionIdentifier == permissionIdentifier);
            if (null != existing) return;
            existing = new Permission { PermissionIdentifier = permissionIdentifier };
            permissionsAdapter.SaveOrUpdate(existing);
        }
    }
}
