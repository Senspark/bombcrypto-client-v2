import React from "react";

export default function MaintenanceDisplay() {
    return <div style={warningStyle}>Server is under maintenance.</div>
}

const warningStyle: React.CSSProperties = {
    color: '#F97068',
    fontSize: '5vw',
    fontWeight: 'bold',
    textAlign: 'center',
    position: 'absolute',
    top: '50%',
    left: '50%',
    transform: 'translate(-50%, -50%)',
    backgroundColor: '#EDF2EF',
    padding: '5vw 10vw', // Adjusted padding for horizontal layout
    borderRadius: '8px',
    border: '1px solid #f5c6cb',
    boxShadow: '0 4px 8px rgba(0, 0, 0, 0.1)',
    maxWidth: '80%', // Adjusted max width for horizontal layout
    boxSizing: 'border-box',
};