import createFetchClient from 'openapi-fetch'
import createClient from 'openapi-react-query'
import type { paths } from './api-schema.gen'

// The API is served from the same origin as the app (proxied by Vite in development).
const fetchClient = createFetchClient<paths>({ baseUrl: window.location.origin })

export const api = createClient(fetchClient)
