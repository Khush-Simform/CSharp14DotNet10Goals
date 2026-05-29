#!/usr/bin/env bash
# verify-scaffold.sh
# Usage: bash .github/skills/scaffold-ca-endpoint/scripts/verify-scaffold.sh <EntityName>
#
# Verifies that all files required for a new CA Management API entity have been
# created and that the shared files (DtoMapper, ServiceCollectionExtensions) were
# updated correctly.
#
# Exit code 0 = all checks passed
# Exit code 1 = one or more checks failed

set -euo pipefail

# ── Argument validation ────────────────────────────────────────────────────────
if [[ $# -lt 1 ]]; then
    echo "Usage: $0 <EntityName>  (e.g. Invoice)"
    exit 1
fi

ENTITY="$1"
ENTITIES="${ENTITY}s"   # naïve pluralisation — override if irregular (e.g. Category → Categories)

# Allow caller to supply custom plural: verify-scaffold.sh Category Categories
if [[ $# -ge 2 ]]; then
    ENTITIES="$2"
fi

PASS=0
FAIL=0

# ── Helpers ────────────────────────────────────────────────────────────────────
check_file() {
    local path="$1"
    if [[ -f "$path" ]]; then
        echo "  ✅  $path"
        ((PASS++))
    else
        echo "  ❌  MISSING: $path"
        ((FAIL++))
    fi
}

check_content() {
    local file="$1"
    local pattern="$2"
    local label="$3"
    if [[ -f "$file" ]] && grep -q "$pattern" "$file"; then
        echo "  ✅  $label"
        ((PASS++))
    else
        echo "  ❌  NOT FOUND in $file: $label  (grep: '$pattern')"
        ((FAIL++))
    fi
}

# ── File existence checks ──────────────────────────────────────────────────────
echo ""
echo "━━━ Checking new files for entity: $ENTITY ━━━"

check_file "Models/${ENTITY}.cs"
check_file "DTOs/${ENTITIES}/${ENTITY}Response.cs"
check_file "DTOs/${ENTITIES}/Create${ENTITY}Request.cs"
check_file "Services/I${ENTITY}Service.cs"
check_file "Services/${ENTITY}Service.cs"
check_file "Controllers/${ENTITIES}Controller.cs"

# ── Content checks ─────────────────────────────────────────────────────────────
echo ""
echo "━━━ Checking shared file updates ━━━"

# CaDtoMapper — ToResponse
check_content \
    "Helpers/CaDtoMapper.cs" \
    "this ${ENTITY} " \
    "CaDtoMapper has ToResponse(this ${ENTITY} ...)"

# CaDtoMapper — ToDomain
check_content \
    "Helpers/CaDtoMapper.cs" \
    "this Create${ENTITY}Request" \
    "CaDtoMapper has ToDomain(this Create${ENTITY}Request ...)"

# DI registration
check_content \
    "Configurations/ServiceCollectionExtensions.cs" \
    "I${ENTITY}Service, ${ENTITY}Service" \
    "ServiceCollectionExtensions registers I${ENTITY}Service"

# ── Model quality checks ───────────────────────────────────────────────────────
echo ""
echo "━━━ Checking model code quality ━━━"

MODEL="Models/${ENTITY}.cs"
if [[ -f "$MODEL" ]]; then
    check_content "$MODEL" "sealed class ${ENTITY}"   "Model is sealed"
    check_content "$MODEL" "get; init;"               "Model uses init-only properties"

    # Warn (not fail) on DateTime usage
    if grep -q "\bDateTime\b" "$MODEL"; then
        echo "  ⚠️   WARNING: $MODEL uses DateTime — prefer DateTimeOffset or DateOnly"
    fi
fi

# ── Request DTO quality checks ─────────────────────────────────────────────────
REQUEST_DTO="DTOs/${ENTITIES}/Create${ENTITY}Request.cs"
if [[ -f "$REQUEST_DTO" ]]; then
    check_content "$REQUEST_DTO" "\[Required\]"                         "Request DTO has [Required] annotations"
    check_content "$REQUEST_DTO" "System.ComponentModel.DataAnnotations" "Request DTO imports DataAnnotations"
fi

# ── Controller checks ──────────────────────────────────────────────────────────
CONTROLLER="Controllers/${ENTITIES}Controller.cs"
if [[ -f "$CONTROLLER" ]]; then
    check_content "$CONTROLLER" "\[ApiController\]"    "Controller has [ApiController]"
    check_content "$CONTROLLER" "sealed class"         "Controller is sealed"
    check_content "$CONTROLLER" "I${ENTITY}Service"    "Controller injects I${ENTITY}Service"
    check_content "$CONTROLLER" "ProducesResponseType" "Controller has ProducesResponseType attributes"
fi

# ── Service checks ─────────────────────────────────────────────────────────────
SERVICE="Services/${ENTITY}Service.cs"
if [[ -f "$SERVICE" ]]; then
    check_content "$SERVICE" "sealed class ${ENTITY}Service" "Service is sealed"
    check_content "$SERVICE" "CaDataStore store"             "Service uses primary constructor injection"

    # Warn on async usage
    if grep -q "\basync\b" "$SERVICE"; then
        echo "  ⚠️   WARNING: $SERVICE contains async — this project uses synchronous storage"
    fi
fi

# ── Summary ────────────────────────────────────────────────────────────────────
echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  Passed : $PASS"
echo "  Failed : $FAIL"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

if [[ $FAIL -gt 0 ]]; then
    echo "  ❌  Scaffold incomplete — fix the issues above before committing."
    exit 1
else
    echo "  ✅  All scaffold checks passed!"
    exit 0
fi
