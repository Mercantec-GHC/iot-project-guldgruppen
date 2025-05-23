import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import SensorData from './SensorData';
import SignUp from './SignUp';
import Login from './Login';
import Settings from './Settings';
import { useAuth } from './AuthContext';

/**
 * Beskyttet rute-komponent - sikrer at kun autentificerede brugere kan tilgå visse sider
 * @param {Object} props - Komponent props
 * @param {ReactNode} props.children - Den underliggende komponent der skal beskyttes
 * @returns {ReactNode} Enten børnekomponenten, en loading-indikator eller en omdirigering til login
 */
const ProtectedRoute = ({ children }) => {
    const { isAuthenticated } = useAuth(); // Henter autentificeringsstatus fra kontekst

    // Vis loading mens autentificeringsstatus indlæses
    if (isAuthenticated === null) {
        return <div className="loading-spinner">Loading...</div>;
    }

    // Hvis brugeren er autentificeret, vis den beskyttede side - ellers omdiriger til login
    return isAuthenticated ? children : <Navigate to="/login" replace />;
};

/**
 * Hoved App-komponent - definerer applikationens routing-struktur
 */
function App() {
    return (
        <BrowserRouter> {/* Router-komponent der håndterer navigation */}
            <Routes> {/* Container for alle route-definitioner */}
                {/* Forside - kræver autentificering */}
                <Route
                    path="/"
                    element={
                        <ProtectedRoute>
                            <SensorData /> {/* SensorData vises kun for autentificerede brugere */}
                        </ProtectedRoute>
                    }
                />
                {/* Åbne ruter (uden autentificering) */}
                <Route path="/signup" element={<SignUp />} /> {/* Tilmelding */}
                <Route path="/login" element={<Login />} /> {/* Login */}

                {/* Indstillinger - kræver autentificering */}
                <Route
                    path="/settings"
                    element={
                        <ProtectedRoute>
                            <Settings /> {/* Settings vises kun for autentificerede brugere */}
                        </ProtectedRoute>
                    }
                />
            </Routes>
        </BrowserRouter>
    );
}

export default App;
