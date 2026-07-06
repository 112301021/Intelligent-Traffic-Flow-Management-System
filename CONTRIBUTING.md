# Contributing to Intelligent Traffic Flow Management System

## Getting Started

1. Fork the repository.
2. Create a branch: `git checkout -b feature/your-feature-name`.
3. Open the project in Unity 2021.3 LTS or newer.
4. Make your changes.
5. Push and open a Pull Request.

## Code Style

- Follow C# conventions (PascalCase methods, camelCase variables).
- Keep Unity scripts focused — one responsibility per component.
- Document public methods with XML comments.

## Known Issues (from README)

If you're fixing a known limitation, reference it in your PR:
- Hardcoded file path in `ReadInput.cs:7`
- File polling overhead in `ReadInput.cs:Update()`
- Dead code: `Left_TF.cs`, `Front_TF.cs`

## Pull Request Checklist

- [ ] Project opens and runs in Unity 2021.3+
- [ ] No hardcoded absolute paths
- [ ] No large binary files (.mp4, .mov, .docx)
- [ ] Debug logs removed or guarded
- [ ] Changes are compatible with the external optimizer interface

## Reporting Issues

Open an issue with:
- Unity version
- Error messages (if applicable)
- Steps to reproduce
