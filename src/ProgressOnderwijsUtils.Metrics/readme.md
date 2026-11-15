# ProgressOnderwijsUtils.Metrics

Progress Onderwijs Utils Metrics - An in-process metrics collector.

It listens to the `http.server.request.duration` and `http.server.active_requests` instruments of ASP.NET Core and the
`db.client.operation.duration` instrument of the OpenTelemetry SqlClient instrumentation, and samples the thread pool
and the SqlClient connection pools. Everything is kept per second in lock-free ring buffers, from which a window of
seconds can be collected at any time.

The application supplies the dimensions (which database and which endpoint a measurement belongs to) and decides what
to do with a collection; this package contains no persistence.

Register it at startup with `services.AddProgressOnderwijsMetrics(collector, samplingFailed)`: this enables the
OpenTelemetry SqlClient instrumentation and starts the collector before the web server accepts its first request.
