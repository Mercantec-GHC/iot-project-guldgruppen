import { useState } from 'react';

function SignUp() {
    // States for form data and validation
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [region, setRegion] = useState('Copenhagen'); // Default to Copenhagen
    const [message, setMessage] = useState('');

    // Function to handle form submission
    const handleSubmit = async (e) => {
        e.preventDefault();

        // Create the ArduinoId based on the selected region
        const arduinoId = region === 'Copenhagen'
            ? '123e4567-e89b-12d3-a456-426614174000'
            : '123e4567-e89b-12d3-a456-426614174001';

        // Create the user object to send to the API
        const userData = {
            username,
            email,
            password,
            arduinoId
        };

        // Send POST request to API to create a new user
        try {
            const response = await fetch('http://176.9.37.136:5001/api/Users', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify(userData),
            });

            if (response.ok) {
                setMessage('User created successfully!');
                // Clear the form after successful submission
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
                    <label>Choose Region</label>
                    <select value={region} onChange={(e) => setRegion(e.target.value)}>
                        <option value="Copenhagen">Copenhagen</option>
                        <option value="Skive">Skive</option>
                    </select>
                </div>

                <button type="submit">Sign Up</button>
            </form>

            {message && <p>{message}</p>}
        </div>
    );
}

export default SignUp;