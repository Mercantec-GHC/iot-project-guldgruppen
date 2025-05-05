import { BrowserRouter, Routes, Route } from 'react-router-dom';
import SensorData from './SensorData';
import SignUp from './SignUp';
import Login from './Login';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route path="/" element={<SensorData />} />
                <Route path="/signup" element={<SignUp />} />
                <Route path="/login" element={<Login />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;