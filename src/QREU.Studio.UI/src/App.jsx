import { Link, Route, Routes } from "react-router-dom"
import RunsPage from "./pages/RunPage/RunsPage"
import RunViewPage from "./pages/RunViewPage/RunViewPage"
import './App.css'

function App() {
    return (
        <div className="app-shell">
            <header className="app-header">
                <div className="app-header-left">
                    <Link to="/runs" className="app-title">QREU Studio</Link>
                    <span className="app-subtitle">Run Diagnostics</span>
                </div>
                {/* <div className="app-header-right">
                    <span className="app-pill">Research</span>
                    <span className="app-pill">Live View</span>
                </div> */}
            </header>
            <main className="app-main">
                <Routes>
                    <Route path="/runs" element={<RunsPage />} />
                    <Route path="/runs/:runId" element={<RunViewPage />} />
                </Routes>
            </main>
        </div>
    )
}

export default App