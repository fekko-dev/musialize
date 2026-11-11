import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterAll, afterEach } from 'vitest'
import { server } from './server'

// jsdom doesn't implement scrolling; the router calls it on every navigation.
window.scrollTo = () => {}

// openapi-fetch captures `fetch` when the client is created, so MSW must patch it
// before any test module imports the client (https://openapi-ts.dev/openapi-fetch/testing).
server.listen({ onUnhandledFrame: 'error' })

afterEach(() => {
  cleanup()
  server.resetHandlers()
})

afterAll(() => {
  server.close()
})
