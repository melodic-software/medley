### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
MDYARCH001 | Medley.Architecture | Error | CrossModuleDirectReference
MDYARCH002 | Medley.Architecture | Error | CrossModuleDatabaseAccess
MDYARCH003 | Medley.Architecture | Error | DomainReferencesApplication
MDYARCH004 | Medley.Architecture | Error | DomainReferencesInfrastructure
MDYARCH005 | Medley.Architecture | Error | ApplicationReferencesInfrastructure
MDYARCH006 | Medley.Architecture | Error | ContractsReferencesInternal
MDYNAME001 | Medley.Naming | Warning | RepositoryMissingSuffix (Code fix: AddSuffixCodeFixProvider)
MDYNAME002 | Medley.Naming | Warning | ValidatorMissingSuffix (Code fix: AddSuffixCodeFixProvider)
MDYNAME003 | Medley.Naming | Warning | HandlerMissingSuffix (Code fix: AddSuffixCodeFixProvider)
MDYNAME004 | Medley.Naming | Warning | SpecificationMissingSuffix (Code fix: AddSuffixCodeFixProvider)
MDYNAME005 | Medley.Naming | Info | ServiceMissingSuffix (Code fix: AddSuffixCodeFixProvider)
MDYNAME006 | Medley.Naming | Warning | ConfigurationMissingSuffix (Code fix: AddSuffixCodeFixProvider)
MDYNAME007 | Medley.Naming | Info | DtoMissingSuffix (Code fix: AddSuffixCodeFixProvider)
MDYCQRS001 | Medley.CQRS | Warning | CommandNotReturningResult
MDYCQRS002 | Medley.CQRS | Error | QueryWithSideEffects
MDYCQRS003 | Medley.CQRS | Error | MultipleHandlersForRequest
MDYCQRS004 | Medley.CQRS | Info | HandlerNotInFeaturesFolder
MDYCQRS005 | Medley.CQRS | Info | ValidatorNotWithCommand
MDYDDD001 | Medley.DDD | Error | MutableCollectionExposed
MDYDDD002 | Medley.DDD | Warning | PublicSetterOnEntity
MDYDDD003 | Medley.DDD | Warning | DomainLogicInHandler
MDYDDD004 | Medley.DDD | Warning | AggregateReferenceByObject
MDYDDD005 | Medley.DDD | Error | ValueObjectWithIdentity
MDYRES001 | Medley.Result | Warning | ThrowingForBusinessFailure
MDYRES002 | Medley.Result | Warning | IgnoringResultValue
MDYRES003 | Medley.Result | Error | ResultNotAwaited
MDYASYNC001 | Medley.Async | Warning | MissingCancellationToken
MDYASYNC002 | Medley.Async | Warning | CancellationTokenNotPassed
MDYASYNC003 | Medley.Async | Error | AsyncVoidMethod
