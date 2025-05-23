import { createContext, useContext, useState, useEffect } from 'react';

// Opretter en ny React kontekst til autentificering
const AuthContext = createContext();

/**
 * Authentication Provider komponent - giver autentificeringsfunktionalitet til hele appen
 * @param {Object} props - Komponent props
 * @param {ReactNode} props.children - Komponenter der skal have adgang til autentifikation
 * @returns {ReactNode} Provider komponent der giver adgang til auth state og funktioner
 */
export const AuthProvider = ({ children }) => {
    // State til at holde styr på om brugeren er autentificeret
    // null = ikke bestemt endnu, true = autentificeret, false = ikke autentificeret
    const [isAuthenticated, setIsAuthenticated] = useState(null);

    // Effekt der kører ved komponentmount - tjekker for token i localStorage
    useEffect(() => {
        const token = localStorage.getItem('token'); // Henter token fra browserens localStorage
        setIsAuthenticated(!!token); // Sætter isAuthenticated baseret på om token findes
    }, []);

    /**
     * Logout funktion - fjerner autentificering og token
     */
    const logout = () => {
        setIsAuthenticated(false); // Sætter autentificering til false
        localStorage.removeItem('token'); // Fjerner token fra localStorage
    };

    // Provider komponent der giver adgang til auth state og funktioner
    return (
        <AuthContext.Provider value={{ isAuthenticated, setIsAuthenticated, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

/**
 * Custom hook til nem adgang til autentifikationskonteksten
 * @returns {Object} Autentifikationskonteksten med state og funktioner
 */
export const useAuth = () => useContext(AuthContext);
