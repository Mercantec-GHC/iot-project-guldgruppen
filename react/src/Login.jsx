import { useState } from 'react';
import { useAuth } from './AuthContext';
import { useNavigate, Link } from 'react-router-dom';
import './Login.css'; // Make sure you have this CSS file for styling

function Login() {
    const { setIsAuthenticated } = useAuth();
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');
    const [isLoading, setIsLoading] = useState(false);
    const navigate = useNavigate();

    const handleSubmit = async (e) => {
        e.preventDefault();
        setError(''); // Clear previous errors
        setIsLoading(true);

        try {
            const response = await fetch('http://176.9.37.136:5001/api/Auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ email, password }),
            });

            const data = await response.json();

            if (response.ok) {
                const token = data.token || data.Token; // Handle both cases
                if (!token) {
                    throw new Error('No token received');
                }

                // Store token and set auth state
                localStorage.setItem('token', token);
                setIsAuthenticated(true);

                // Immediately verify the token works
                try {
                    console.log("Received token:", token);
                    const verifyRes = await fetch('http://176.9.37.136:5001/api/Auth/userid', {
                        method: 'GET',
                        headers: {
                            'Authorization': `Bearer ${token}`,
                            'Content-Type': 'application/json'
                        },
                        credentials: 'include' // Add this if using cookies
                    });
                    console.log("Verification response status:", verifyRes.status);

                    if (!verifyRes.ok) {
                        throw new Error('Token verification failed');
                    }

                    // If verification succeeds, navigate to home
                    navigate('/');
                } catch (verifyErr) {
                    localStorage.removeItem('token');
                    setIsAuthenticated(false);
                    setError('Login verification failed. Please try again.');
                    console.error('Token verification error:', verifyErr);
                }
            } else {
                throw new Error(data.message || 'Login failed');
            }
        } catch (err) {
            setError(err.message || 'Invalid credentials. Please try again.');
            console.error('Login error:', err);
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className="login-container">
            <h2>Login</h2>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label>Email</label>
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                        disabled={isLoading}
                    />
                </div>
                <div className="form-group">
                    <label>Password</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                        disabled={isLoading}
                    />
                </div>
                <button type="submit" disabled={isLoading}>
                    {isLoading ? 'Logging in...' : 'Login'}
                </button>
            </form>
            {error && <p className="error">{error}</p>}
            <div className="signup-link">
                <p>Don't have an account? <Link to="/signup">Sign up here</Link>.</p>
            </div>
        </div>
    );
}

export default Login;
