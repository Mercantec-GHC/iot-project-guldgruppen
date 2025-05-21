import { createContext, useContext, useState, useEffect } from 'react';

// Opretter en ny Context til autentificering
const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
    // Opretter en ny Context til autentificering
    const [isAuthenticated, setIsAuthenticated] = useState(null);

    useEffect(() => {
        // Tjekker, om der findes en token i localStorage ved første render
        const token = localStorage.getItem('token');
        setIsAuthenticated(!!token);
    }, []);

    const logout = () => {
        // Logger brugeren ud ved at nulstille state og fjerne token
        setIsAuthenticated(false);
        localStorage.removeItem('token');
    };

    return (
        <AuthContext.Provider value={{ isAuthenticated, setIsAuthenticated, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => useContext(AuthContext);
