import { createOpenApiHttp } from 'openapi-msw'
import type { paths } from '../lib/api-schema.gen'

// MSW's `http`, typed from the generated OpenAPI schema: handlers only accept the API's
// real paths and responses, so a changed endpoint breaks the tests' type check.
export const http = createOpenApiHttp<paths>()
