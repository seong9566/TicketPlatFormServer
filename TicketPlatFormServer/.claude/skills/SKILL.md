---
name: codex-claude-loop
description: Orchestrates a dual-AI engineering loop where Claude Code plans and implements, while Codex validates and reviews, with continuous feedback for optimal code quality
---

# Codex-Claude Engineering Loop Skill

## Overview
This skill implements a balanced engineering loop where Claude Code handles planning and implementation, while Codex provides validation and code review. The continuous feedback loop ensures optimal code quality.

## Core Workflow

### Phase 1: Planning with Claude Code
I'll start by creating a detailed implementation plan:

1. Analyze the task requirements
2. Break down the implementation into clear, actionable steps
3. Document assumptions and identify potential issues
4. Create a structured plan document

### Phase 2: Plan Validation with Codex
Before implementing, I'll validate the plan with Codex:

1. First, I'll ask you to choose Codex settings:
   - Model: `gpt-5.2` or `gpt-5.2-codex`
   - Reasoning effort: `low`, `medium`, or `high`

2. Then I'll send the plan to Codex for validation:
   ```bash
   echo "Review this implementation plan and identify any issues:

   [Claude's plan here]

   Check for:
   - Logic errors
   - Missing edge cases
   - Architecture flaws
   - Security concerns
   - Performance considerations" | codex exec -m <model> --config model_reasoning_effort=<effort> --sandbox read-only
   ```

3. Analyze Codex's feedback and address any concerns

### Phase 3: Feedback Loop
If Codex identifies issues:

1. I'll summarize the concerns
2. Refine the plan based on feedback
3. Ask you: "Should I revise the plan and re-validate, or proceed with addressing the issues during implementation?"
4. Repeat validation if needed

### Phase 4: Implementation
Once the plan is validated:

1. Implement the code using Edit, Write, and Read tools
2. Break down implementation into manageable steps
3. Execute each step with proper error handling
4. Track progress with TodoWrite
5. Document what was implemented

### Phase 5: Cross-Review After Changes
After implementation:

1. Send the implementation to Codex for comprehensive review:
   ```bash
   echo "Review this implementation for:
   - Bugs and logic errors
   - Performance issues
   - Security vulnerabilities
   - Best practices violations
   - Code quality concerns

   [Implementation details]" | codex exec -m <model> --config model_reasoning_effort=<effort> --sandbox read-only
   ```

2. Analyze Codex's feedback and decide:
   - Apply fixes immediately for critical issues
   - Discuss with you if architectural changes are needed
   - Document all decisions made

### Phase 6: Iterative Improvement
1. Apply necessary fixes based on Codex's review
2. For significant changes, re-validate with Codex:
   ```bash
   echo "Verify the fixes address the previous concerns" | codex exec resume --last
   ```
3. Continue the loop until code quality standards are met

## Recovery When Issues Are Found

### When Codex Identifies Problems:
1. Analyze the root cause
2. Implement fixes using available tools
3. Send updated code back to Codex for verification
4. Repeat until validation passes

### When Implementation Errors Occur:
1. Review the error/issue
2. Adjust implementation strategy
3. Re-validate with Codex before proceeding

## Best Practices

- **Always validate plans** before execution
- **Never skip cross-review** after changes
- **Maintain clear handoff** between AIs
- **Document who did what** for context
- **Use resume** to preserve session state and settings
- **Ask for user input** when architectural decisions are needed

## Command Reference

| Phase | Command Pattern | Purpose |
|-------|----------------|---------|
| Validate plan | `echo "plan" \| codex exec --sandbox read-only` | Check logic before coding |
| Implement | Claude uses Edit/Write/Read tools | Claude implements the validated plan |
| Review code | `echo "review changes" \| codex exec --sandbox read-only` | Codex validates Claude's implementation |
| Continue review | `echo "next step" \| codex exec resume --last` | Continue validation session (inherits all settings) |
| Apply fixes | Claude uses Edit/Write tools | Claude fixes issues found by Codex |
| Re-validate | `echo "verify fixes" \| codex exec resume --last` | Codex re-checks after fixes |

## Error Handling

1. Stop on non-zero exit codes from Codex
2. Summarize Codex feedback and ask for direction via `AskUserQuestion`
3. Before implementing changes, confirm approach with user if:
   - Significant architectural changes needed
   - Multiple files will be affected
   - Breaking changes are required
4. When Codex warnings appear, evaluate severity and decide next steps

## The Perfect Loop

```
Plan (Claude) → Validate Plan (Codex) → Feedback →
Implement (Claude) → Review Code (Codex) →
Fix Issues (Claude) → Re-validate (Codex) → Repeat until perfect
```

This creates a self-correcting, high-quality engineering system where:
- **Claude Code** handles all code implementation and modifications
- **Codex** provides validation, review, and quality assurance
- **Continuous feedback** ensures optimal code quality

## Usage Example

```
User: Implement user authentication feature
Claude: I'll use the codex-claude-loop skill to ensure high-quality implementation.

[Phase 1: Planning]
Creating implementation plan...
- Set up authentication middleware
- Create user login/register endpoints
- Implement JWT token generation
- Add authentication guards
- Write tests

[Phase 2: Validation]
Asking for Codex settings...
Sending plan to Codex for validation...

[Codex feedback received]
Codex suggests adding refresh token logic and rate limiting.

[Phase 3: Feedback]
Updating plan with Codex's suggestions...
Re-validating with Codex...

[Phase 4: Implementation]
Plan validated! Starting implementation...
[TodoWrite to track progress]

[Phase 5: Cross-Review]
Implementation complete. Sending to Codex for review...

[Codex feedback: Security concern with password hashing]

[Phase 6: Improvement]
Fixing security issue...
Re-validating with Codex...
All checks passed!
```

## Notes

- This skill requires `codex` CLI to be installed and configured
- The `--sandbox read-only` flag ensures Codex only reads files for review
- Using `resume --last` preserves model selection, reasoning effort, and all other settings from the original session
- Always maintain clear communication with the user about what each AI is doing
