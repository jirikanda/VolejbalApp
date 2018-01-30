import * as React from 'react';

const Spinner = (props) =>
    <div className={`spinner-backdrop ${props.className}`}>
        <div className="spinner-wrapper">
            <div className="spinner"></div>
        </div>
    </div>
export default Spinner;