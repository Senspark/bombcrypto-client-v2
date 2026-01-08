import React from "react";

export default function BlockIpDisplay() {
    return <div style={warningStyle}>Your region is not supported.</div>
}

const warningStyle: React.CSSProperties = {
    color: 'red',
    fontSize: '5vw',
    fontWeight: 'bold',
    textAlign: 'center',
    position: 'absolute',
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    backgroundColor: 'white',
    padding: '5vw 10vw', // Adjusted padding for horizontal layout
    borderRadius: '8px',
    border: '1px solid #f5c6cb',
    boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
    maxWidth: '80%', // Adjusted max width for horizontal layout
    boxSizing: 'border-box',
};