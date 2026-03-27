import { UUID } from "std:uuid";

let AccountID = newType(UUID);

class Account 
    : UniqueIDMixin[AccountID]
    , AuditAllMixin
    of ORMModel(table: "accounts")
{
    @unique(
        name: "Unique Account Name"
    )
    var name: String(min=1, max=255, regex="^[a-z0-9-]+$");

    @relationship(
        filter: not Workspace.isDeleted,
        UniqueIDMixin: .Select,
        orderBy: [Descending(Workspace.createdAt)],
    )
    var workspaces: List<Workspace>;
}
