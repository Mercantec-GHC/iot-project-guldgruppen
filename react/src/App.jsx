import { BrowserRouter, Routes, Route } from 'react-router-dom';
import SensorData from './SensorData';
import SignUp from './SignUp';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<SensorData />} />
                <Route path="/signup" element={<SignUp />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;