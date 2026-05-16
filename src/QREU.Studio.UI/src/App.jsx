import { Link, Route, Routes } from "react-router-dom"
import RunsPage from "./pages/RunsPage"
import RunViewPage from "./pages/RunViewPage"

function App() {
    return (
        <div>
        <header style={{padding:12, borderBottom:'1px solid #eee'}}>
            <Link to="/runs"><h1>QREU Studio</h1></Link>
        </header>
        <main style={{padding:12}}>
            <Routes>
                <Route path="/runs" element={<RunsPage />} />
                <Route path="/runs/:runId" element={<RunViewPage />} />
            </Routes>
        </main>
    </div>
    )
}

export default App