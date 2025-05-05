import { useState } from 'react';
import './SignUp.css';

function SignUp() {
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [region, setRegion] = useState('Copenhagen');
    const [message, setMessage] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();

        const arduinoId = region === 'Copenhagen'
            ? '123e4567-e89b-12d3-a456-426614174000'
            : region === 'Skive'
                ? '123e4567-e89b-12d3-a456-426614174001'
                : '123e4567-e89b-12d3-a456-426614174002';

        const userData = {
            username,
            email,
            password,
            arduinoId
        };

        try {
            const response = await fetch('http://176.9.37.136:5001/api/Auth/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(userData),
            });

            if (response.ok) {
                setMessage('User created successfully!');
                setUsername('');
                setEmail('');
                setPassword('');
            } else {
                setMessage('Error creating user. Please try again.');
            }
        } catch (error) {
            setMessage('Error: ' + error.message);
        }
    };

    return (
        <div className="signup-container">
            <h2>Sign Up</h2>
            <form onSubmit={handleSubmit}>
                <div className="form-group">
                    <label>Username</label>
                    <input
                        type="text"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        required
                    />
                </div>

                <div className="form-group">
                    <label>Email</label>
                    <input
                        type="email"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        required
                    />
                </div>

                <div className="form-group">
                    <label>Password</label>
                    <input
                        type="password"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        required
                    />
                </div>

                <div className="form-group">
                    <label>Select the Office Location You Wish to Receive Environmental Sensor Data From</label>
                    <select value={region} onChange={(e) => setRegion(e.target.value)}>
                        <option value="Copenhagen">Copenhagen</option>
                        <option value="Skive">Skive</option>
                        <option value="Aalborg">Aalborg</option>
                    </select>
                </div>

                <button type="submit">Sign Up</button>
            </form>

            {message && <p>{message}</p>}
        </div>
    );
}

export default SignUp;