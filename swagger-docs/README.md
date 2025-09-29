# Swagger API Documentation

This project provides Swagger documentation for the API defined in the `server.py` file. The documentation is structured in the `openapi.yaml` file, which describes the API endpoints, request and response formats, authentication methods, and data models used in the application.

## Getting Started

To view the API documentation, you can use the `openapi.yaml` file with any compatible Swagger UI or OpenAPI viewer. Follow the steps below to get started:

1. **Install Swagger UI**: You can either host Swagger UI locally or use an online version. For local hosting, you can clone the Swagger UI repository from GitHub.

2. **Load the Documentation**:
   - If using a local instance, place the `openapi.yaml` file in the `dist` folder of the Swagger UI.
   - Open `index.html` in your browser and point it to the `openapi.yaml` file.

3. **Explore the API**: Once the documentation is loaded, you can explore the various endpoints, view request and response formats, and test the API directly from the Swagger UI.

## API Overview

The API provides the following functionalities:

- User registration and login
- Parking lot management
- Reservation management
- Vehicle management
- Payment processing

## Authentication

The API uses token-based authentication. After logging in, users receive a session token that must be included in the `Authorization` header for subsequent requests.

## Additional Information

For more details on the API endpoints, request and response formats, and data models, please refer to the `openapi.yaml` file. This file contains comprehensive information about each endpoint, including:

- HTTP methods (GET, POST, PUT, DELETE)
- Required parameters and request bodies
- Response formats and status codes

Feel free to contribute to the documentation or the API itself by submitting issues or pull requests.