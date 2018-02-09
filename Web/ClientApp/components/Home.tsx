import * as React from 'react'
import Counter from '../containers/Counter'
import Configuration from '../configuration'

const Home = () => <div>
    <h1>NewProjectTemplate</h1>
    <Counter />
    <h2>
        Web configuration
    </h2>
    <code>
        {JSON.stringify(Configuration)}
    </code>
</div>;

export default Home