import { DateTime as _DateTime } from "std:datetime";
import { UTC } from "std:timezone";


let DateTime = _DateTime[UTC];


class CreatedAtMixin of Mixins {
    @column(
        factory: DateTime.now,
    )
    let createdAt: DateTime;
}


class DeletedAtMixin of Mixins {
    var deletedAt: DateTime? = null;

    @query()
    get isDeleted(cls): SQLExpression[Bool]
        => cls.deletedAt is not null;

    fun markDeleted({
        at: DateTime | null = null,
    }) {
        deletedAt = at or DateTime.now();
    }
}


class UpdatedAtMixin of Mixins {
    @onUpdate(
        factory: DateTime.now,
    )
    var updatedAt: DateTime;
}


class AuditAllMixin : CreatedAtMixin, UpdatedAtMixin, DeletedAtMixin;
