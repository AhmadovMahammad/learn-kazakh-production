# Learn Kazakh Language

A free and comprehensive platform for learning Kazakh from scratch to B1 conversational level. This project is built on real personal learning experience and aims to make Kazakh language resources open and accessible to everyone.

## Quick Start

### Prerequisites

- .NET 8.0 or later
- Docker and Docker Compose (for containerized setup)

### Running with Docker (Recommended)

1. **Clone the repository**

   ```bash
   git clone https://github.com/yourusername/learn-kazakh-production.git
   cd learn-kazakh-production
   ```

2. **Configure environment variables**

   ```bash
   # Copy sample environment files
   cp docker/app.env.server.sample docker/app.env.server
   cp docker/app.env.postgres.sample docker/app.env.postgres

   # Edit the configuration files as needed
   nano docker/app.env.server
   nano docker/app.env.postgres
   ```

3. **Run the application**

   ```bash
   docker-compose up -d
   ```

4. **Access the application**

   - Frontend: [http://localhost:8081](http://localhost:8081)
   - API: [http://localhost:8080](http://localhost:8080)

### Running Locally

1. **Configure appsettings.json**

   ```bash
   cd src/server/LearnKazakh.API
   # Update appsettings.json with your database connection and other settings
   ```

2. **Run the API**

   ```bash
   cd src/server/LearnKazakh.API
   dotnet run
   ```

3. **Run the Client**

   ```bash
   cd src/client
   dotnet run
   ```

### Technology Stack

- **Backend**: ASP.NET Core Web API
- **Frontend**: Blazor WebAssembly
- **Database**: PostgreSQL
- **Containerization**: Docker & Docker Compose
- **Reverse Proxy**: Nginx

## Contributing

This is an open-source project and contributions of any kind are welcome. You can add new learning materials, improve the documentation, fix bugs, enhance the UI, or even translate content. Every contribution, big or small, helps make this platform better for learners.

### How to Contribute

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Live Demo

The platform is available online at: [www.learnkz.com](https://www.learnkz.com)

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Contact

If you have suggestions or questions about the platform, feel free to reach out. All feedback is welcome, especially if you’d like to share your own experience with learning Kazakh.

## Support

This platform is free and will always remain free. If you’d like to support its growth, you can:

- Star the repository on GitHub
- Share it with others who are interested in learning Kazakh
- Contribute to the codebase
- Support me directly via [Buy Me a Coffee](https://buymeacoffee.com/ahmadovmahammad)

---

**Note**: Learn Kazakh is not a commercial course but a personal open-source effort to share a real learning journey with others.
