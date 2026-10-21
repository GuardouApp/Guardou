<h1 align="center">Guardou!</h1>
<br/>
<p align="center">
  <img src="design/screenshot.webp" width="720" title="Main Screenshot" alt="Main Screenshot">
</p>
<br/>

## About

Guardou is a self-hosted file storage solution. Designed to be a simple alternative for users who want to store and organize some files through a simple web ui.

**Core Features:**
- Multiple users
- SSO Support

No telemetry. No tracking. No ads. 100% Free.

## Features

| Feature                    | Status |
|----------------------------|--------|
| Multiple users             | Yes    | 
| File/Folder Sharing        | WIP    |
| OIDC-based SSO             | Yes    |

## Getting Started

Use the [docker-compose](docker/docker-compose.yaml) file provided in the repository. No changes are required but you may customize it for your needs.

Run the container:

```
docker-compose up -d
```


## Tech Stack

| Tool    | Version        |
|---------|----------------|
| .NET    | 10             |
| C#      | 14             |
| Flutter | 3.47.7         |

## AI Disclaimer

Guardou is a personal experiment where I leverage my professional knowledge to test if it's possible to build a stable software using AI.
AI is writing **only the code**. Documentation, architeture, features, UI/UX, and technology choices are up to me.

I'm personally not a fan of AI generated stuff, that's why the server is on C# and not Rust 😅 I'm using something I actually understand, trying my best to keep track of everything, writing small, focused PR's, and following best practices.

That said, I still consider Guardou a **vibe-coded app**. For this reason, I don't recommended it for production use. **Use it at your own risk.**

Finally, if you decide to use it, please follow [3-2-1](https://www.backblaze.com/blog/the-3-2-1-backup-strategy/) backup strategy.
